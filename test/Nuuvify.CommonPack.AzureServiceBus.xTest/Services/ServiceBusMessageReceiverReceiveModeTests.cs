namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

/// <summary>
/// Testes unitários para o suporte a ReceiveMode no ServiceBusMessageReceiver
/// Valida que as operações de settlement são ignoradas no modo ReceiveAndDelete
/// </summary>
[Trait("Category", "Unit")]
public class ServiceBusMessageReceiverReceiveModeTests
{
    private readonly Mock<ILogger<TestServiceBusMessageReceiver>> _loggerMock;
    private readonly Mock<IConfigurationCustom> _configMock;
    private readonly RequestConfiguration _requestConfig;
    private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test";

    public ServiceBusMessageReceiverReceiveModeTests()
    {
        _loggerMock = new Mock<ILogger<TestServiceBusMessageReceiver>>();
        _configMock = new Mock<IConfigurationCustom>();
        _requestConfig = new RequestConfiguration { CorrelationId = Guid.NewGuid().ToString() };

        _ = _configMock.Setup(x => x.GetSectionValue(It.IsAny<string>()))
            .Returns(ValidConnectionString);
    }

    #region ReceiveMode Default Tests

    [Fact]
    public async Task ReceiveMode_Default_ShouldBePeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);

        // Act
        var receiveMode = receiver.TestReceiveMode;

        // Assert
        receiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
    }

    [Fact]
    public async Task IsReceiveAndDeleteMode_Default_ShouldBeFalse()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);

        // Act
        var isReceiveAndDelete = receiver.TestIsReceiveAndDeleteMode;

        // Assert
        isReceiveAndDelete.ShouldBeFalse();
    }

    #endregion

    #region ReceiveMode Configuration - Queue Tests

    [Fact]
    public async Task ConfigureServiceBusQueue_WithPeekLock_ShouldSetReceiveModePeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };

        // Act
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfigureServiceBusQueue_WithReceiveAndDelete_ShouldSetReceiveModeReceiveAndDelete()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfigureServiceBusQueue_WithNullOptions_ShouldDefaultToPeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);

        // Act
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue");

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfigureServiceBusQueue_WithConfigName_WithReceiveAndDelete_ShouldSetReceiveMode()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        receiver.TestConfigureServiceBusQueue("TestConnection", "TestQueue", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
    }

    #endregion

    #region ReceiveMode Configuration - Topic Tests

    [Fact]
    public async Task ConfigureServiceBusTopic_WithPeekLock_ShouldSetReceiveModePeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        };

        // Act
        receiver.ConfigureServiceBusForTopic(ValidConnectionString, "test-topic", "test-subscription", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfigureServiceBusTopic_WithReceiveAndDelete_ShouldSetReceiveModeReceiveAndDelete()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        receiver.ConfigureServiceBusForTopic(ValidConnectionString, "test-topic", "test-subscription", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
    }

    [Fact]
    public async Task ConfigureServiceBusTopic_WithNullOptions_ShouldDefaultToPeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);

        // Act
        receiver.ConfigureServiceBusForTopic(ValidConnectionString, "test-topic", "test-subscription");

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeFalse();
    }

    [Fact]
    public async Task ConfigureServiceBusTopic_WithConfigName_WithReceiveAndDelete_ShouldSetReceiveMode()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        };

        // Act
        receiver.TestConfigureServiceBus("TestConnection", "TestTopic", "TestSubscription", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
    }

    #endregion

    #region ReceiveMode Reconfiguration Tests

    [Fact]
    public async Task ConfigureServiceBus_Reconfigure_ShouldUpdateReceiveMode()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);

        // Act - Configure first with PeekLock
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);

        // Act - Reconfigure with ReceiveAndDelete
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue-2", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
    }

    #endregion

    #region HandleMessageAsync - ReceiveAndDelete Tests

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_SuccessfulProcessing_ShouldNotCallComplete()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-001"
        );

        var mockProcessor = new Mock<ServiceBusReceiver>();
        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção (em PeekLock lançaria pois o mock não está configurado)
        await Should.NotThrowAsync(() => receiver.TestHandleMessageAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_FailedProcessing_ShouldNotCallDeadLetterOrAbandon()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: false);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-002"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção (em PeekLock lançaria pois o mock não está configurado)
        await Should.NotThrowAsync(() => receiver.TestHandleMessageAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WithAbandonFlag_FailedProcessing_ShouldNotCallAbandon()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: false, abandonIfFailed: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test message"),
            messageId: "test-msg-003"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção (em PeekLock lançaria pois o mock não está configurado)
        await Should.NotThrowAsync(() => receiver.TestHandleMessageAsync(args, CancellationToken.None));
    }

    #endregion

    #region Exception Handling - ReceiveAndDelete Tests

    [Fact]
    public async Task HandleBusinessLogicFailure_ReceiveAndDelete_ShouldNotCallSettlement()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: false);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-bl-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção
        await Should.NotThrowAsync(() => receiver.TestHandleBusinessLogicFailureAsync(args, CancellationToken.None));
    }

    [Fact]
    public async Task HandleServiceBusSpecificException_ReceiveAndDelete_ShouldNotCallDeadLetter()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
        await Should.NotThrowAsync(() => receiver.TestHandleServiceBusSpecificExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleServiceBusCommunicationException_ReceiveAndDelete_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
            () => receiver.TestHandleServiceBusCommunicationExceptionAsync(args, ex, CancellationToken.None));

        exception.Message.ShouldContain("Erro de comunicação no Service Bus");
    }

    [Fact]
    public async Task HandleOperationCanceledException_ReceiveAndDelete_ShouldNotCallSettlement()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
        await Should.NotThrowAsync(() => receiver.TestHandleOperationCancelledExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleOperationCanceledException_ReceiveAndDelete_WithAbandonFlag_ShouldNotCallSettlement()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true, abandonIfFailed: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
        await Should.NotThrowAsync(() => receiver.TestHandleOperationCancelledExceptionAsync(args, ex, CancellationToken.None));
    }

    [Fact]
    public async Task HandleGenericException_ReceiveAndDelete_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiverWithResult(_loggerMock.Object, _configMock.Object, _requestConfig, executeResult: true);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
            () => receiver.TestHandleGenericExceptionAsync(args, ex, CancellationToken.None));

        exception.Message.ShouldContain("Erro não tratado durante processamento da mensagem");
    }

    #endregion

    #region HandleMessageAsync - Exception in ReceiveAndDelete Tests

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WhenExecuteThrows_ShouldThrowWithoutDeadLetter()
    {
        // Arrange
        var thrownException = new InvalidOperationException("Processing failed");
        await using var receiver = new TestServiceBusMessageReceiverWithException(
            _loggerMock.Object, _configMock.Object, _requestConfig, thrownException);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
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
            () => receiver.TestHandleMessageAsync(args, CancellationToken.None));

        exception.Message.ShouldContain("Erro não tratado durante processamento da mensagem");
    }

    [Fact]
    public async Task HandleMessageAsync_ReceiveAndDelete_WhenExecuteThrowsOperationCanceled_ShouldNotThrow()
    {
        // Arrange
        var thrownException = new OperationCanceledException("Cancelled");
        await using var receiver = new TestServiceBusMessageReceiverWithException(
            _loggerMock.Object, _configMock.Object, _requestConfig, thrownException);
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData("test"),
            messageId: "test-msg-cancel-001"
        );

        var args = CreateProcessMessageEventArgs(message);

        // Act & Assert - Não deve lançar exceção pois OperationCanceled é handled
        await Should.NotThrowAsync(() => receiver.TestHandleMessageAsync(args, CancellationToken.None));
    }

    #endregion

    #region PeekLock Comparison Tests

    [Fact]
    public async Task ReceiveMode_PeekLock_AfterQueueConfig_ShouldRemainPeekLock()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 10
        };

        // Act
        receiver.ConfigureServiceBusForQueue(ValidConnectionString, "test-queue", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.PeekLock);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeFalse();
    }

    [Fact]
    public async Task ReceiveMode_ReceiveAndDelete_AfterTopicConfig_ShouldBeReceiveAndDelete()
    {
        // Arrange
        await using var receiver = new TestServiceBusMessageReceiver(_loggerMock.Object, _configMock.Object, _requestConfig);
        var processorOptions = new ServiceBusProcessorOptions
        {
            ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
            MaxConcurrentCalls = 5
        };

        // Act
        receiver.ConfigureServiceBusForTopic(ValidConnectionString, "test-topic", "test-sub", processorOptions);

        // Assert
        receiver.TestReceiveMode.ShouldBe(ServiceBusReceiveMode.ReceiveAndDelete);
        receiver.TestIsReceiveAndDeleteMode.ShouldBeTrue();
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
}

/// <summary>
/// Implementação de teste que permite controlar o resultado de ExecuteReceivedMessageAsync
/// </summary>
public class TestServiceBusMessageReceiverWithResult : TestServiceBusMessageReceiver
{
    private readonly bool _executeResult;

    public TestServiceBusMessageReceiverWithResult(
        ILogger<TestServiceBusMessageReceiver> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        bool executeResult,
        bool abandonIfFailed = false)
        : base(logger, configurationCustom, requestConfiguration)
    {
        _executeResult = executeResult;
        AbandonMessageIfFailed = abandonIfFailed;
    }

    public override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return _executeResult;
    }
}

/// <summary>
/// Implementação de teste que lança exceções no ExecuteReceivedMessageAsync
/// </summary>
public class TestServiceBusMessageReceiverWithException : TestServiceBusMessageReceiver
{
    private readonly Exception _exception;

    public TestServiceBusMessageReceiverWithException(
        ILogger<TestServiceBusMessageReceiver> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        Exception exception)
        : base(logger, configurationCustom, requestConfiguration)
    {
        _exception = exception;
    }

    public override Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        throw _exception;
    }
}
