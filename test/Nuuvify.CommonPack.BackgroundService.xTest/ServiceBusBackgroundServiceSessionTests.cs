using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService.Services;
using Nuuvify.CommonPack.BackgroundService.xTest.Fakers;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Shouldly;
using System.Diagnostics;
using Xunit;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Testes do processamento de filas com sessões e da correlação no construtor sem RequestConfiguration.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ServiceBusBackgroundServiceSessionTests : IDisposable
{
    private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test";
    private const string CnnName = "ServiceBus:CnnName";
    private const string QueueName = "session-queue";

    private readonly Mock<ILogger<TestSessionServiceBusBackgroundService>> _loggerMock = new();
    private readonly Mock<IConfigurationCustom> _configurationMock;
    private readonly Mock<ServiceBusSessionReceiver> _sessionReceiverMock = new();
    private readonly ActivitySource _activitySource = new("SessionTestActivitySource");

    public ServiceBusBackgroundServiceSessionTests()
    {
        _configurationMock = ServiceBusBackgroundServiceFaker.GenerateConfigurationMock();
        _ = _configurationMock.Setup(x => x.GetSectionValue(It.IsAny<string>()))
            .Returns(ValidConnectionString);
    }

    [Fact]
    public void ConfigureServiceBusSession_WithEmptyQueueName_ShouldThrowArgumentException()
    {
        using var service = CreateService();

        var ex = Should.Throw<ArgumentException>(() =>
            service.TestConfigureServiceBusSession(CnnName, string.Empty, new ServiceBusSessionProcessorOptions()));

        ex.Message.ShouldContain("QueueName");
    }

    [Fact]
    public void ConfigureServiceBusSession_WhenConnectionStringIsMissingInVault_ShouldThrowArgumentException()
    {
        _ = _configurationMock.Setup(x => x.GetSectionValue(It.IsAny<string>())).Returns(string.Empty);
        using var service = CreateService();

        var ex = Should.Throw<ArgumentException>(() =>
            service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions()));

        ex.Message.ShouldContain("Vault");
    }

    [Fact]
    public void ConfigureServiceBusSession_WithValidConfiguration_ShouldApplyReceiveModeFromOptions()
    {
        using var service = CreateService();

        service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        service.GetIsReceiveAndDeleteMode().ShouldBeTrue();
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenProcessingSucceeds_ShouldCompleteMessage()
    {
        using var service = CreateConfiguredService();
        var message = CreateMessage("msg-success");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        VerifyNotDeadLettered(message);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenBusinessLogicFails_ShouldDeadLetterWithDiagnostics()
    {
        using var service = CreateConfiguredService();
        service.SetExecuteRuleResult(false);
        var message = CreateMessage("msg-business-failure");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["ExceptionType"] == "BusinessLogicFailure"),
            It.IsAny<CancellationToken>()), Times.Once);
        _sessionReceiverMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenBusinessLogicFailsAndAbandonIsEnabled_ShouldAbandonMessage()
    {
        using var service = CreateConfiguredService();
        service.SetExecuteRuleResult(false);
        service.SetAbandonMessageIfFailed(true);
        var message = CreateMessage("msg-abandon");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.AbandonMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["AbandonReason"] == "Business logic returned false"),
            It.IsAny<CancellationToken>()), Times.Once);
        VerifyNotDeadLettered(message);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_InReceiveAndDeleteMode_ShouldNotSettleMessage()
    {
        using var service = CreateService();
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });
        service.SetExecuteRuleResult(false);
        var message = CreateMessage("msg-receive-and-delete");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Never);
        _sessionReceiverMock.Verify(r => r.AbandonMessageAsync(
            message,
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        VerifyNotDeadLettered(message);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenOperationIsCanceledAndAbandonIsEnabled_ShouldAbandonMessage()
    {
        using var service = CreateConfiguredService();
        service.SetThrowOperationCanceledException();
        service.SetAbandonMessageIfFailed(true);
        var message = CreateMessage("msg-canceled");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.AbandonMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["AbandonReason"] == "Operation was cancelled"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenSessionLockIsLost_ShouldDeadLetterMessage()
    {
        using var service = CreateConfiguredService();
        service.SetThrowServiceBusException(ServiceBusFailureReason.SessionLockLost);
        var message = CreateMessage("msg-session-lock-lost");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["ExceptionType"] == nameof(ServiceBusException)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WithoutActivitySource_ShouldThrowArgumentException()
    {
        using var service = CreateService();
        service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions());

        _ = await Should.ThrowAsync<ArgumentException>(() =>
            service.TestHandleSessionMessageAsync(CreateArgs(CreateMessage("msg-no-activity")), CancellationToken.None));
    }

    [Fact]
    public void CreateDeadLetterProperties_WithoutRequestConfiguration_ShouldUseMessageCorrelationId()
    {
        using var service = CreateService();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("payload"),
            messageId: "msg-correlation",
            correlationId: "message-correlation-id");

        var properties = service.TestCreateDeadLetterProperties(message);

        properties["CorrelationId"].ShouldBe("message-correlation-id");
    }

    [Fact]
    public void CreateAbandonProperties_WithoutAnyCorrelationId_ShouldUseUnknownValue()
    {
        using var service = CreateService();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("payload"),
            messageId: "msg-no-correlation");

        var properties = service.TestCreateAbandonProperties(message);

        properties["CorrelationId"].ShouldBe("Unknown");
    }

    [Fact]
    public async Task HandleBusinessLogicFailureAsync_WithoutSession_ShouldStillDeadLetterThroughReceiver()
    {
        var receiverMock = new Mock<ServiceBusReceiver>();
        using var service = CreateService();
        service.TestConfigureServiceBus(CnnName, QueueName);
        var message = CreateMessage("msg-non-session");

        await service.TestHandleBusinessLogicFailureAsync(
            new ProcessMessageEventArgs(message, receiverMock.Object, CancellationToken.None),
            CancellationToken.None);

        receiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["ExceptionType"] == "BusinessLogicFailure"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private TestSessionServiceBusBackgroundService CreateService()
        => new(_loggerMock.Object, _configurationMock.Object);

    private TestSessionServiceBusBackgroundService CreateConfiguredService()
    {
        var service = CreateService();
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions());
        return service;
    }

    private ProcessSessionMessageEventArgs CreateArgs(ServiceBusReceivedMessage message)
        => new(message, _sessionReceiverMock.Object, CancellationToken.None);

    private static ServiceBusReceivedMessage CreateMessage(string messageId)
        => ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("payload"),
            messageId: messageId,
            sessionId: "session-a");

    private void VerifyNotDeadLettered(ServiceBusReceivedMessage message)
        => _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);

    public void Dispose() => _activitySource.Dispose();
}

/// <summary>
/// Implementação de teste que usa o construtor canônico, sem RequestConfiguration.
/// </summary>
public sealed class TestSessionServiceBusBackgroundService : ServiceBusBackgroundService<TestSessionServiceBusBackgroundService>
{
    private bool _executeRuleResult = true;
    private ServiceBusFailureReason? _throwServiceBusException;
    private bool _throwOperationCanceledException;

    public TestSessionServiceBusBackgroundService(
        ILogger<TestSessionServiceBusBackgroundService> logger,
        IConfigurationCustom configurationCustom)
        : base(logger, configurationCustom)
    {
    }

    protected override Task<bool> ExecuteReceivedMessageAsync(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        if (_throwServiceBusException.HasValue)
        {
            throw new ServiceBusException("Test ServiceBus exception", _throwServiceBusException.Value);
        }

        if (_throwOperationCanceledException)
        {
            throw new OperationCanceledException("Test operation was cancelled");
        }

        return Task.FromResult(_executeRuleResult);
    }

    public void SetActivitySource(ActivitySource activitySource) => ActivitySourceCustom = activitySource;

    public void SetExecuteRuleResult(bool result) => _executeRuleResult = result;

    public void SetThrowServiceBusException(ServiceBusFailureReason reason) => _throwServiceBusException = reason;

    public void SetThrowOperationCanceledException() => _throwOperationCanceledException = true;

    public void SetAbandonMessageIfFailed(bool abandon) => AbandonMessageIfFailed = abandon;

    public bool GetIsReceiveAndDeleteMode() => IsReceiveAndDeleteMode;

    public void TestConfigureServiceBusSession(string cnnName, string queueName, ServiceBusSessionProcessorOptions options)
        => ConfigureServiceBusSession(cnnName, queueName, new ServiceBusClientOptions(), options);

    public void TestConfigureServiceBus(string cnnName, string queueName)
        => ConfigureServiceBus(cnnName, queueName, new ServiceBusClientOptions(), new ServiceBusProcessorOptions());

    public Task TestHandleSessionMessageAsync(ProcessSessionMessageEventArgs args, CancellationToken cancellationToken)
        => HandleSessionMessageAsync(args, cancellationToken);

    public Task TestHandleBusinessLogicFailureAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
        => HandleBusinessLogicFailureAsync(args, cancellationToken);

    public Dictionary<string, object> TestCreateDeadLetterProperties(ServiceBusReceivedMessage message)
        => CreateDeadLetterProperties(message, "error");

    public Dictionary<string, object> TestCreateAbandonProperties(ServiceBusReceivedMessage message)
        => CreateAbandonProperties(message, "reason");
}
