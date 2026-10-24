using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService.Services;
using Nuuvify.CommonPack.BackgroundService.xTest.Fakers;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Shouldly;
using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Testes de configuração por credencial, execução e ciclo de vida do processamento com sessões.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ServiceBusBackgroundServiceSessionLifecycleTests : IDisposable
{
    private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test";
    private const string CnnName = "ServiceBus:CnnName";
    private const string QueueName = "session-queue";
    private const string FullyQualifiedNamespace = "test.servicebus.windows.net";

    private readonly Mock<ILogger<TestSessionServiceBusBackgroundService>> _loggerMock = new();
    private readonly Mock<IConfigurationCustom> _configurationMock;
    private readonly Mock<ServiceBusSessionReceiver> _sessionReceiverMock = new();
    private readonly ActivitySource _activitySource = new("SessionLifecycleTestActivitySource");

    public ServiceBusBackgroundServiceSessionLifecycleTests()
    {
        _configurationMock = ServiceBusBackgroundServiceFaker.GenerateConfigurationMock();
        _ = _configurationMock.Setup(x => x.GetSectionValue(It.IsAny<string>()))
            .Returns(ValidConnectionString);
    }

    [Fact]
    public void ConfigureServiceBusSession_WithEmptyConnectionName_ShouldThrowArgumentException()
    {
        using var service = CreateService();

        var ex = Should.Throw<ArgumentException>(() =>
            service.TestConfigureServiceBusSession(string.Empty, QueueName, new ServiceBusSessionProcessorOptions()));

        ex.Message.ShouldContain("ConnectionString");
    }

    [Theory]
    [InlineData("", FullyQualifiedNamespace, true, "QueueName")]
    [InlineData(QueueName, "", true, "FullyQualifiedNamespace")]
    [InlineData(QueueName, FullyQualifiedNamespace, false, "TokenCredential")]
    public void ConfigureServiceBusSessionWithCredential_WithInvalidArguments_ShouldThrowArgumentException(
        string queueName,
        string fullyQualifiedNamespace,
        bool hasCredential,
        string expectedMessageFragment)
    {
        using var service = CreateService();
        var credential = hasCredential ? ServiceBusBackgroundServiceFaker.GenerateTokenCredentialMock().Object : null!;

        var ex = Should.Throw<ArgumentException>(() =>
            service.TestConfigureServiceBusSession(queueName, fullyQualifiedNamespace, credential, new ServiceBusSessionProcessorOptions()));

        ex.Message.ShouldContain(expectedMessageFragment);
    }

    [Fact]
    public void ConfigureServiceBusSessionWithCredential_WithValidArguments_ShouldApplyReceiveModeWithoutRequestingToken()
    {
        var credentialMock = ServiceBusBackgroundServiceFaker.GenerateTokenCredentialMock();
        using var service = CreateService();

        service.TestConfigureServiceBusSession(QueueName, FullyQualifiedNamespace, credentialMock.Object, new ServiceBusSessionProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        service.GetIsReceiveAndDeleteMode().ShouldBeTrue();
        credentialMock.Verify(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Never);
        credentialMock.Verify(c => c.GetToken(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenServiceCommunicationFails_ShouldDeadLetterAndThrow()
    {
        using var service = CreateConfiguredService();
        service.SetThrowServiceBusException(ServiceBusFailureReason.ServiceCommunicationProblem);
        var message = CreateMessage("msg-communication-problem");

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None));

        ex.Message.ShouldContain("msg-communication-problem");
        ex.InnerException.ShouldBeOfType<ServiceBusException>();
        _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["ExceptionType"] == nameof(ServiceBusException)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenServiceCommunicationFailsInReceiveAndDeleteMode_ShouldNotSettleNorThrow()
    {
        using var service = CreateService();
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBusSession(CnnName, QueueName, new ServiceBusSessionProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });
        service.SetThrowServiceBusException(ServiceBusFailureReason.ServiceCommunicationProblem);
        var message = CreateMessage("msg-communication-receive-and-delete");

        await Should.NotThrowAsync(() => service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None));

        _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleSessionMessageAsync_WhenOperationIsCanceledAndAbandonIsDisabled_ShouldDeadLetterMessage()
    {
        using var service = CreateConfiguredService();
        service.SetThrowOperationCanceledException();
        var message = CreateMessage("msg-canceled-dead-letter");

        await service.TestHandleSessionMessageAsync(CreateArgs(message), CancellationToken.None);

        _sessionReceiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["ExceptionType"] == nameof(OperationCanceledException)),
            It.IsAny<CancellationToken>()), Times.Once);
        _sessionReceiverMock.Verify(r => r.AbandonMessageAsync(
            message,
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleBusinessLogicFailureAsync_WithoutSessionAndAbandonEnabled_ShouldAbandonThroughReceiver()
    {
        var receiverMock = new Mock<ServiceBusReceiver>();
        using var service = CreateService();
        service.TestConfigureServiceBus(CnnName, QueueName);
        service.SetAbandonMessageIfFailed(true);
        var message = CreateMessage("msg-non-session-abandon");

        await service.TestHandleBusinessLogicFailureAsync(
            new ProcessMessageEventArgs(message, receiverMock.Object, CancellationToken.None),
            CancellationToken.None);

        receiverMock.Verify(r => r.AbandonMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["AbandonReason"] == "Business logic returned false"),
            It.IsAny<CancellationToken>()), Times.Once);
        receiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.IsAny<IDictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutConfiguration_ShouldThrowInvalidOperationException()
    {
        using var service = CreateService();

        var ex = await Should.ThrowAsync<InvalidOperationException>(() => service.TestExecuteAsync(CancellationToken.None));

        ex.Message.ShouldContain("ConfigureServiceBusSession");
    }

    [Fact]
    public async Task ExecuteAsync_WithSessionConfigured_WhenStoppingTokenIsCanceled_ShouldFinishGracefully()
    {
        using var service = CreateConfiguredService();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.NotThrowAsync(() => service.TestExecuteAsync(cts.Token));

        VerifyLogged<OperationCanceledException>(LogLevel.Information, Times.Once());
        VerifyNothingLogged(LogLevel.Error);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSessionConfigured_WhenStoppingTokenIsCanceled_ShouldFinishGracefully()
    {
        using var service = CreateService();
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus(CnnName, QueueName);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.NotThrowAsync(() => service.TestExecuteAsync(cts.Token));

        VerifyLogged<OperationCanceledException>(LogLevel.Information, Times.Once());
        VerifyNothingLogged(LogLevel.Error);
    }

    [Fact]
    public async Task StopAsync_WithSessionProcessor_ShouldStopAndDisposeSessionProcessor()
    {
        var sessionProcessorMock = new Mock<ServiceBusSessionProcessor>();
        using var service = CreateService();
        InjectSessionProcessor(service, sessionProcessorMock.Object);

        await service.StopAsync(CancellationToken.None);

        sessionProcessorMock.Verify(p => p.StopProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
        sessionProcessorMock.Verify(p => p.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyNothingLogged(LogLevel.Warning);
    }

    [Theory]
    [InlineData(nameof(ObjectDisposedException))]
    [InlineData(nameof(InvalidOperationException))]
    [InlineData(nameof(ServiceBusException))]
    public async Task StopAsync_WhenSessionProcessorFailsToStop_ShouldLogWarningAndStillDispose(string exceptionType)
    {
        var failure = CreateException(exceptionType);
        var sessionProcessorMock = new Mock<ServiceBusSessionProcessor>();
        _ = sessionProcessorMock
            .Setup(p => p.StopProcessingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure);
        using var service = CreateService();
        InjectSessionProcessor(service, sessionProcessorMock.Object);

        await Should.NotThrowAsync(() => service.StopAsync(CancellationToken.None));

        VerifyLogged(LogLevel.Warning, failure, Times.Once());
        sessionProcessorMock.Verify(p => p.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_WithoutSessionProcessor_ShouldReleaseResourcesWithoutWarnings()
    {
        using var service = CreateService();
        service.TestConfigureServiceBus(CnnName, QueueName);

        await Should.NotThrowAsync(() => service.StopAsync(CancellationToken.None));

        VerifyNothingLogged(LogLevel.Warning);
        VerifyNothingLogged(LogLevel.Error);
    }

    [Theory]
    [InlineData(nameof(ObjectDisposedException))]
    [InlineData(nameof(InvalidOperationException))]
    public void Dispose_WhenSessionProcessorDisposeFails_ShouldLogWarningWithoutThrowing(string exceptionType)
    {
        var failure = CreateException(exceptionType);
        var sessionProcessorMock = new Mock<ServiceBusSessionProcessor>();
        _ = sessionProcessorMock.Setup(p => p.CloseAsync(It.IsAny<CancellationToken>())).ThrowsAsync(failure);
        using var service = CreateService();
        InjectSessionProcessor(service, sessionProcessorMock.Object);

        Should.NotThrow(service.Dispose);

        VerifyLogged(LogLevel.Warning, failure, Times.Once());
        sessionProcessorMock.Verify(p => p.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleBusinessLogicFailureAsync_WithRequestConfiguration_ShouldPreferRequestCorrelationId()
    {
        var requestConfiguration = new RequestConfiguration { CorrelationId = "request-correlation-id" };
        var receiverMock = new Mock<ServiceBusReceiver>();
        using var service = new TestServiceBusBackgroundService(
            new Mock<ILogger<TestServiceBusBackgroundService>>().Object,
            _configurationMock.Object,
            requestConfiguration);
        service.TestConfigureServiceBus(CnnName, QueueName, new ServiceBusClientOptions(), new ServiceBusProcessorOptions());
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("payload"),
            messageId: "msg-request-correlation",
            correlationId: "message-correlation-id");

        await service.TestHandleBusinessLogicFailureAsync(
            new ProcessMessageEventArgs(message, receiverMock.Object, CancellationToken.None),
            CancellationToken.None);

        receiverMock.Verify(r => r.DeadLetterMessageAsync(
            message,
            It.Is<IDictionary<string, object>>(p => (string)p["CorrelationId"] == "request-correlation-id"),
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

    private static Exception CreateException(string exceptionType) => exceptionType switch
    {
        nameof(ObjectDisposedException) => new ObjectDisposedException("session-processor"),
        nameof(InvalidOperationException) => new InvalidOperationException("session processor failure"),
        nameof(ServiceBusException) => new ServiceBusException("session processor failure", ServiceBusFailureReason.GeneralError),
        _ => throw new ArgumentOutOfRangeException(nameof(exceptionType), exceptionType, null)
    };

    // O cliente é criado internamente pela classe base, então o processador simulado é injetado no campo privado.
    private static void InjectSessionProcessor(TestSessionServiceBusBackgroundService service, ServiceBusSessionProcessor processor)
    {
        var field = typeof(ServiceBusBackgroundService<TestSessionServiceBusBackgroundService>)
            .GetField("_serviceBusSessionProcessor", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Campo _serviceBusSessionProcessor não encontrado.");

        field.SetValue(service, processor);
    }

    private void VerifyLogged(LogLevel level, Exception exception, Times times)
        => _loggerMock.Verify(l => l.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

    private void VerifyLogged<TException>(LogLevel level, Times times)
        where TException : Exception
        => _loggerMock.Verify(l => l.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.Is<Exception>(e => e is TException),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

    private void VerifyNothingLogged(LogLevel level)
        => _loggerMock.Verify(l => l.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never());

    public void Dispose() => _activitySource.Dispose();
}
