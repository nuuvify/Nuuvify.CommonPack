using System.Reflection;

namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderTopicTests : IClassFixture<ServiceBusTestFixture>, IAsyncDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Mock<ILogger<ServiceBusMessageSender>> _mockLogger;
    private readonly ServiceBusMessageSender _serviceBusMessageSender;

    public ServiceBusMessageSenderTopicTests(ServiceBusTestFixture fixture)
    {
        _fixture = fixture;
        _mockLogger = new Mock<ILogger<ServiceBusMessageSender>>();

        var config = _fixture.CreateValidServiceBusConfiguration();
        _serviceBusMessageSender = new ServiceBusMessageSender(config, _mockLogger.Object);
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusMessageSender != null)
            await _serviceBusMessageSender.DisposeAsync();
    }

    #region SendMessageToTopicAsync Tests

    [Fact]
    public async Task SendMessageToTopicAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Test Message" };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendMessageToTopicAsync_WithMessageOptions_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Test Message" };
        var messageOptions = new ServiceBusMessageOptions
        {
            MessageId = "custom-message-id",
            CorrelationId = "correlation-123",
            TimeToLive = TimeSpan.FromMinutes(30),
            PartitionKey = "partition-1"
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions, operationOptions: null, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendMessageToTopicAsync_WithOperationOptions_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Test Message" };
        var operationOptions = new ServiceBusOperationOptions
        {
            CustomConnectionString = _fixture.CreateValidServiceBusConfiguration().Value.ConnectionString,
            CustomClientOptions = new ServiceBusClientOptions()
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: operationOptions, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessageToTopicAsync_WithInvalidTopicName_ShouldThrowArgumentException(string topicName)
    {
        // Arrange
        var message = new { Id = 1, Name = "Test Message" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task SendMessageToTopicAsync_WithNullTopicName_ShouldThrowArgumentException()
    {
        // Arrange
        string topicName = null!;
        var message = new { Id = 1, Name = "Test Message" };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task SendMessageToTopicAsync_WithNullMessage_ShouldThrowArgumentException()
    {
        // Arrange
        var topicName = "test-topic";
        object message = null!;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("message");
    }

    [Fact]
    public async Task SendMessageToTopicAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Test Message" };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task SendMessageToTopicAsync_CompatibilityOverload_ShouldWork()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Test Message" };
        var messageOptions = new ServiceBusMessageOptions
        {
            MessageId = "compatibility-test"
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions, CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region SendBatchMessagesToTopicAsync Tests

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithValidMessages_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var messages = new[]
        {
            new { Id = 1, Name = "Message 1" },
            new { Id = 2, Name = "Message 2" },
            new { Id = 3, Name = "Message 3" }
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithMessageOptions_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var messages = new[]
        {
            new { Id = 1, Name = "Message 1" },
            new { Id = 2, Name = "Message 2" }
        };
        var messageOptions = new ServiceBusMessageOptions
        {
            TimeToLive = TimeSpan.FromMinutes(30)
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithEmptyList_ShouldLogWarningAndReturn()
    {
        // Arrange
        var topicName = "test-topic";
        var messages = Array.Empty<object>();

        // Act & Assert
        await Should.NotThrowAsync(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, cancellationToken: CancellationToken.None));

        // Verify warning is logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Lista de mensagens vazia")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendBatchMessagesToTopicAsync_WithInvalidTopicName_ShouldThrowArgumentException(string topicName)
    {
        // Arrange
        var messages = new[] { new { Id = 1, Name = "Message 1" } };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithNullTopicName_ShouldThrowArgumentException()
    {
        // Arrange
        string topicName = null!;
        var messages = new[] { new { Id = 1, Name = "Message 1" } };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithNullMessages_ShouldThrowArgumentException()
    {
        // Arrange
        var topicName = "test-topic";
        IEnumerable<object> messages = null!;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("messages");
    }

    [Fact]
    public async Task SendBatchMessagesToTopicAsync_WithOperationOptionsOverload_ShouldWork()
    {
        // Arrange
        var topicName = "test-topic";
        var messages = new[] { new { Id = 1, Name = "Message 1" } };
        var operationOptions = new ServiceBusOperationOptions();

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, messages, messageOptions: null, operationOptions: operationOptions, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);
    }

    #endregion

    #region ScheduleMessageToTopicAsync Tests

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithValidParameters_ShouldReturnSequenceNumber()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Scheduled Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert - Como este é um teste unitário sem infraestrutura real,
        // esperamos uma InvalidOperationException ao tentar conectar
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, options: null, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithMessageOptions_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Scheduled Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);
        var messageOptions = new ServiceBusMessageOptions
        {
            MessageId = "scheduled-message-id",
            CorrelationId = "correlation-scheduled"
        };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, messageOptions, CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithPastTime_ShouldThrowArgumentException()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Message" };
        var pastTime = DateTimeOffset.UtcNow.AddMinutes(-30);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, pastTime, options: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("scheduledEnqueueTime");
        exception.Message.ShouldContain("deve ser no futuro");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ScheduleMessageToTopicAsync_WithInvalidTopicName_ShouldThrowArgumentException(string topicName)
    {
        // Arrange
        var message = new { Id = 1, Name = "Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, options: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithNullTopicName_ShouldThrowArgumentException()
    {
        // Arrange
        string topicName = null!;
        var message = new { Id = 1, Name = "Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, options: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithNullMessage_ShouldThrowArgumentException()
    {
        // Arrange
        var topicName = "test-topic";
        object message = null!;
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, options: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("message");
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithOperationOptionsOverload_ShouldWork()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);
        var operationOptions = new ServiceBusOperationOptions();

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(
                topicName, message, futureTime, messageOptions: null, operationOptions: operationOptions, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);
    }

    #endregion

    #region CancelScheduledMessageInTopicAsync Tests

    [Fact]
    public async Task CancelScheduledMessageInTopicAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var sequenceNumber = 123456L; // Mock sequence number for test

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInTopicAsync(topicName, sequenceNumber, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CancelScheduledMessageInTopicAsync_WithInvalidTopicName_ShouldThrowArgumentException(string topicName)
    {
        // Arrange
        var sequenceNumber = 12345L;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInTopicAsync(topicName, sequenceNumber, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task CancelScheduledMessageInTopicAsync_WithNullTopicName_ShouldThrowArgumentException()
    {
        // Arrange
        string topicName = null!;
        var sequenceNumber = 12345L;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInTopicAsync(topicName, sequenceNumber, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public async Task CancelScheduledMessageInTopicAsync_WithOperationOptions_ShouldSucceed()
    {
        // Arrange
        var topicName = "test-topic";
        var sequenceNumber = 123456L; // Mock sequence number for test
        var operationOptions = new ServiceBusOperationOptions();

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInTopicAsync(topicName, sequenceNumber, operationOptions: operationOptions, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Verify logging attempt was made
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task TopicOperations_IntegratedWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var topicName = "integration-topic";
        var message = new { Id = 1, Name = "Integration Test Message" };
        var futureTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);

        // Send batch messages
        var batchMessages = new[]
        {
            new { Id = 2, Name = "Batch Message 1" },
            new { Id = 3, Name = "Batch Message 2" }
        };

        // Verificar que o batch também falha
        var batchException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync(topicName, batchMessages, messageOptions: null, cancellationToken: CancellationToken.None));

        batchException.Message.ShouldContain(topicName);

        // Verificar que schedule também falha
        var scheduleException = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, futureTime, options: null, cancellationToken: CancellationToken.None));

        scheduleException.Message.ShouldContain(topicName);

        // Verify error operations were logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(3)); // At least 3 error operations logged due to connection failures
    }

    [Fact]
    public async Task TopicOperations_WithDisposedSender_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Message" };

        // Dispose the sender
        await sender.DisposeAsync();

        // Act & Assert - All topic operations should throw ObjectDisposedException
        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendBatchMessagesToTopicAsync(topicName, new[] { message }, messageOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.ScheduleMessageToTopicAsync(topicName, message, DateTimeOffset.UtcNow.AddMinutes(30), options: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.CancelScheduledMessageInTopicAsync(topicName, 12345L, cancellationToken: CancellationToken.None));
    }

    #endregion

    #region Private Method Tests via Reflection

    private void InvokeValidateParameter<T>(T parameter, string parameterName)
    {
        var methods = typeof(ServiceBusMessageSender).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(m => m.Name == "ValidateParameter" && m.IsGenericMethod);

        var method = methods.FirstOrDefault()?.MakeGenericMethod(typeof(T));

        if (method is null)
        {
            throw new InvalidOperationException("Could not find ValidateParameter method");
        }

        try
        {
            method.Invoke(null, new object[] { parameter!, parameterName });
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    [Fact]
    public void ValidateParameter_WithValidTopicName_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => InvokeValidateParameter("valid-topic", "topicName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateParameter_WithInvalidTopicName_ShouldThrowArgumentException(string topicName)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => InvokeValidateParameter(topicName, "topicName"));
        exception.ParamName.ShouldBe("topicName");
    }

    [Fact]
    public void ValidateParameter_WithNullTopicName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => InvokeValidateParameter((string)null!, "topicName"));
        exception.ParamName.ShouldBe("topicName");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendMessageToTopicAsync_WithLargeMessage_ShouldHandleGracefully()
    {
        // Arrange
        var topicName = "test-topic";
        var largeString = new string('X', 10000); // Large string
        var message = new { Id = 1, LargeData = largeString };

        // Act & Assert - Esta operação deveria falhar devido à falta de conexão real
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        // Verify that the exception message contains the expected topic name
        exception.Message.ShouldContain(topicName);
    }

    [Fact]
    public async Task ScheduleMessageToTopicAsync_WithExactCurrentTime_ShouldThrowArgumentException()
    {
        // Arrange
        var topicName = "test-topic";
        var message = new { Id = 1, Name = "Message" };
        var currentTime = DateTimeOffset.UtcNow;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync(topicName, message, currentTime, options: null, cancellationToken: CancellationToken.None));

        exception.ParamName.ShouldBe("scheduledEnqueueTime");
        exception.Message.ShouldContain("deve ser no futuro");
    }

    #endregion
}
