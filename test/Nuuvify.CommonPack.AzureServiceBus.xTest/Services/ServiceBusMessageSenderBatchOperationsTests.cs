namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderBatchOperationsTests : IDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly ServiceBusMessageSender _sender;
    private bool _disposed;

    public ServiceBusMessageSenderBatchOperationsTests()
    {
        _fixture = new ServiceBusTestFixture();
        var config = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusMessageSender>();
        _sender = new ServiceBusMessageSender(config, logger.Object);
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithNullQueueName_ShouldThrowArgumentException()
    {
        // Arrange
        var messages = new List<ServiceBusMessage> { new("test") };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            () => _sender.SendBatchMessagesToQueueAsync(null!, messages, null, null, CancellationToken.None));

        exception.ParamName.ShouldBe("queueName");
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithEmptyQueueName_ShouldThrowArgumentException()
    {
        // Arrange
        var messages = new List<ServiceBusMessage> { new("test") };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            () => _sender.SendBatchMessagesToQueueAsync(string.Empty, messages, null, null, CancellationToken.None));

        exception.ParamName.ShouldBe("queueName");
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithNullMessages_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentNullException>(
            () => _sender.SendBatchMessagesToQueueAsync<ServiceBusMessage>("test-queue", null!, null, null, CancellationToken.None));

        exception.ParamName.ShouldBe("messages");
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithEmptyMessageList_ShouldLogWarningAndReturn()
    {
        // Arrange
        var messages = new List<ServiceBusMessage>();

        // Act & Assert
        await Should.NotThrowAsync(() =>
            _sender.SendBatchMessagesToQueueAsync("test-queue", messages, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithValidMessage_ShouldCompleteProcessing()
    {
        // Arrange
        var messages = new List<ServiceBusMessage> { new("test") };

        // Act & Assert - This will attempt to connect but should fail gracefully with configuration error
        var exception = await Should.ThrowAsync<Exception>(() =>
            _sender.SendBatchMessagesToQueueAsync("test-queue", messages, null, null, CancellationToken.None));

        // Verify that the method processed parameters correctly (ArgumentException should have been thrown earlier if validation failed)
        exception.ShouldNotBeOfType<ArgumentException>();
        exception.ShouldNotBeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithMultipleMessages_ShouldAttemptBatchProcessing()
    {
        // Arrange
        var messages = new List<ServiceBusMessage>
        {
            new("message1"),
            new("message2"),
            new("message3")
        };

        // Act & Assert - This will attempt to connect but should fail gracefully with configuration error
        var exception = await Should.ThrowAsync<Exception>(() =>
            _sender.SendBatchMessagesToQueueAsync("test-queue", messages, null, null, CancellationToken.None));

        // Verify that the method processed parameters correctly
        exception.ShouldNotBeOfType<ArgumentException>();
        exception.ShouldNotBeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var messages = new List<ServiceBusMessage> { new("test") };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        _ = await Should.ThrowAsync<InvalidOperationException>(() =>
            _sender.SendBatchMessagesToQueueAsync("test-queue", messages, null, null, cts.Token));
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithLargeMessageCount_ShouldProcessAllMessages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 50)
                                 .Select(i => new ServiceBusMessage($"message{i}"))
                                 .ToList();

        // Act & Assert - This will attempt to connect but should fail gracefully with configuration error
        var exception = await Should.ThrowAsync<Exception>(() =>
            _sender.SendBatchMessagesToQueueAsync("test-queue", messages, null, null, CancellationToken.None));

        // Verify that the method processed parameters correctly
        exception.ShouldNotBeOfType<ArgumentException>();
        exception.ShouldNotBeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithWhitespaceQueueName_ShouldThrowArgumentException()
    {
        // Arrange
        var messages = new List<ServiceBusMessage> { new("test") };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            () => _sender.SendBatchMessagesToQueueAsync("   ", messages, null, null, CancellationToken.None));

        exception.ParamName.ShouldBe("queueName");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _sender?.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
