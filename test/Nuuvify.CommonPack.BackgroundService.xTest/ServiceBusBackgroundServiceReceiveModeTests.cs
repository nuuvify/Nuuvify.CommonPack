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
/// Testes unitários para o suporte a ReceiveMode no ServiceBusBackgroundService
/// Valida que as operações de settlement são ignoradas no modo ReceiveAndDelete
/// </summary>
[Trait("Category", "Unit")]
public sealed class ServiceBusBackgroundServiceReceiveModeTests : IDisposable
{
    private readonly Mock<ILogger<TestServiceBusBackgroundService>> _loggerMock;
    private readonly Mock<IConfigurationCustom> _configurationMock;
    private readonly RequestConfiguration _requestConfiguration;
    private readonly ActivitySource _activitySource;
    private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test";

    public ServiceBusBackgroundServiceReceiveModeTests()
    {
        _loggerMock = new Mock<ILogger<TestServiceBusBackgroundService>>();
        _configurationMock = ServiceBusBackgroundServiceFaker.GenerateConfigurationMock();
        _requestConfiguration = new RequestConfiguration
        {
            CorrelationId = Guid.NewGuid().ToString()
        };
        _activitySource = new ActivitySource("TestActivitySource");

        _ = _configurationMock.Setup(x => x.GetSectionValue(It.IsAny<string>()))
            .Returns(ValidConnectionString);
    }

    #region ReceiveMode Default Tests

    [Fact]
    public void ReceiveMode_Default_ShouldBePeekLock()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act
        var receiveMode = service.GetReceiveMode();

        // Assert
        receiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
    }

    [Fact]
    public void IsReceiveAndDeleteMode_Default_ShouldBeFalse()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act
        var isReceiveAndDelete = service.GetIsReceiveAndDeleteMode();

        // Assert
        isReceiveAndDelete.ShouldBeFalse();
    }

    #endregion

    #region ReceiveMode Configuration - Queue Tests

    [Fact]
    public void ConfigureServiceBusQueue_WithPeekLock_ShouldSetReceiveModePeekLock()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };

        // Act
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, processorOptions);

        // Assert
        service.GetReceiveMode().ShouldBe(ServiceBusReceiveMode.PeekLock);
        service.GetIsReceiveAndDeleteMode().ShouldBeFalse();
    }

    [Fact]
    public void ConfigureServiceBusQueue_WithReceiveAndDelete_ShouldSetReceiveModeReceiveAndDelete()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, processorOptions);

        // Assert
        service.GetReceiveMode().ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        service.GetIsReceiveAndDeleteMode().ShouldBeTrue();
    }

    [Fact]
    public void ConfigureServiceBusQueue_WithNullOptions_ShouldDefaultToPeekLock()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, null);

        // Assert
        service.GetReceiveMode().ShouldBe(ServiceBusReceiveMode.PeekLock);
        service.GetIsReceiveAndDeleteMode().ShouldBeFalse();
    }

    #endregion

    #region ReceiveMode Configuration - Topic Tests

    [Fact]
    public void ConfigureServiceBusTopic_WithReceiveAndDelete_ShouldSetReceiveModeReceiveAndDelete()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        service.TestConfigureServiceBusWithTopicConnectionString("ServiceBus:CnnName", "test-topic", "test-subscription", null, processorOptions);

        // Assert
        service.GetReceiveMode().ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        service.GetIsReceiveAndDeleteMode().ShouldBeTrue();
    }

    [Fact]
    public void ConfigureServiceBusTopic_WithPeekLock_ShouldSetReceiveModePeekLock()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };

        // Act
        service.TestConfigureServiceBusWithTopicConnectionString("ServiceBus:CnnName", "test-topic", "test-subscription", null, processorOptions);

        // Assert
        service.GetReceiveMode().ShouldBe(ServiceBusReceiveMode.PeekLock);
        service.GetIsReceiveAndDeleteMode().ShouldBeFalse();
    }

    #endregion

    #region HandleMessageAsync - ReceiveAndDelete Tests

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_SuccessfulProcessing_ShouldNotCallComplete()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(true);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção (em PeekLock lançaria pois o mock não está configurado)
        await Should.NotThrowAsync(() => service.TestHandleMessageAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_FailedProcessing_ShouldNotCallDeadLetterOrAbandon()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(false);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-002"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleMessageAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WithAbandonFlag_FailedProcessing_ShouldNotCallAbandon()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(false);
        service.SetAbandonMessageIfFailed(true);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-003"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleMessageAsync(args, CancellationToken.None));
    }

    #endregion

    #region Exception Handling - ReceiveAndDelete Tests

    [Fact]
    public async Task HandleBusinessLogicFailure_ReceiveAndDelete_ShouldNotCallSettlement()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-bl-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleBusinessLogicFailureAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleServiceBusSpecificException_ReceiveAndDelete_ShouldNotCallDeadLetter()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-sbs-001"
        );

        var args = CreateProcessMessageEventArgs(message);
        var ex = new ServiceBusException("Test error", ServiceBusFailureReason.MessageLockLost);

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleServiceBusSpecificExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleServiceBusCommunicationException_ReceiveAndDelete_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-comm-001"
        );

        var args = CreateProcessMessageEventArgs(message);
        var ex = new ServiceBusException("Communication error", ServiceBusFailureReason.ServiceCommunicationProblem);

        // Act & Assert - Deve lançar InvalidOperationException mas sem chamar DeadLetter
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => service.TestHandleServiceBusCommunicationExceptionAsync(args, ex, CancellationToken.None));

        exception.Message.ShouldContain("Erro de comunicação no Service Bus");
    }

    [Fact]
    public async Task HandleOperationCanceledException_ReceiveAndDelete_ShouldNotCallSettlement()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-oc-001"
        );

        var args = CreateProcessMessageEventArgs(message);
        var ex = new OperationCanceledException("Test cancellation");

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleOperationCanceledExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleOperationCanceledException_ReceiveAndDelete_WithAbandonFlag_ShouldNotCallSettlement()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetAbandonMessageIfFailed(true);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-oc-002"
        );

        var args = CreateProcessMessageEventArgs(message);
        var ex = new OperationCanceledException("Test cancellation");

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => service.TestHandleOperationCanceledExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleGenericException_ReceiveAndDelete_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-gen-001"
        );

        var args = CreateProcessMessageEventArgs(message);
        var ex = new InvalidOperationException("Generic test error");

        // Act & Assert - Deve lançar InvalidOperationException mas sem chamar DeadLetter
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => service.TestHandleGenericExceptionAsync(args, ex, CancellationToken.None));

        exception.Message.ShouldContain("Erro não tratado durante processamento da mensagem");
    }

    #endregion

    #region HandleMessageAsync - Exception in ReceiveAndDelete Tests

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WhenExecuteThrows_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowGenericException(true);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-exc-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Deve propagar a exceção sem tentar DeadLetter
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => service.TestHandleMessageAsync(args, CancellationToken.None));

        exception.Message.ShouldContain("Erro não tratado durante processamento da mensagem");
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WhenExecuteThrowsOperationCanceled_ShouldNotThrow()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowOperationCanceledException(true);
        service.TestConfigureServiceBus("ServiceBus:CnnName", "test-queue", null, new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-cancel-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção pois OperationCanceled é handled
        await Should.NotThrowAsync(() => service.TestHandleMessageAsync(args, CancellationToken.None));
    }

    #endregion

    #region Helper Methods

    private static ProcessMessageEventArgs CreateProcessMessageEventArgs(ServiceBusReceivedMessage message)
    {
        return new ProcessMessageEventArgs(
            message: message,
            receiver: null,
            cancellationToken: CancellationToken.None);
    }

    #endregion

    public void Dispose()
    {
        _activitySource?.Dispose();
    }
}
