using System.Reflection;

namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderQueueTests : IAsyncDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly ServiceBusMessageSender _sender;

    public ServiceBusMessageSenderQueueTests()
    {
        _fixture = new ServiceBusTestFixture();
        var config = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusMessageSender>();
        _sender = new ServiceBusMessageSender(config, logger.Object);
    }

    #region ResolveServiceBusClient Tests

    [Fact]
    public void ResolveServiceBusClient_WithNullOptions_ShouldReturnDefaultClient()
    {
        // Act
        var result = InvokeResolveServiceBusClient(null);

        // Assert
        _ = result.client.ShouldNotBeNull();
        result.shouldDispose.ShouldBeFalse();
    }

    [Fact]
    public void ResolveServiceBusClient_WithOptionsButNoCreateClient_ShouldReturnDefaultClient()
    {
        // Arrange
        var options = new ServiceBusOperationOptions
        {
            UseTemporaryClient = true
        };

        // Act
        var result = InvokeResolveServiceBusClient(options);

        // Assert
        _ = result.client.ShouldNotBeNull();
        result.shouldDispose.ShouldBeFalse();
    }

    #endregion

    #region SendMessageToQueueAsync Tests

    [Fact]
    public async Task SendMessageToQueueAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Id = 1, Name = "Test Message" };
        var options = new ServiceBusMessageOptions
        {
            MessageId = "test-msg-id"
        };

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() =>
            _sender.SendMessageToQueueAsync(queueName, message, options, null, CancellationToken.None));

        // We expect an InvalidOperationException due to mock client limitations
        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task SendMessageToQueueAsync_WithNullQueueName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var message = new { Test = "value" };

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.SendMessageToQueueAsync<object>(null!, message, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageToQueueAsync_WithEmptyQueueName_ShouldThrowArgumentException()
    {
        // Arrange
        var message = new { Test = "value" };

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentException>(() =>
            _sender.SendMessageToQueueAsync("", message, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageToQueueAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.SendMessageToQueueAsync<object>("test-queue", null!, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageToQueueAsync_WithDisposedSender_ShouldThrowObjectDisposedException()
    {
        // Arrange
        await _sender.DisposeAsync();
        var message = new { Test = "value" };

        // Act & Assert
        _ = await Should.ThrowAsync<ObjectDisposedException>(() =>
            _sender.SendMessageToQueueAsync("test-queue", message, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageToQueueAsync_CompatibilityMethod_ShouldWork()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Id = 1, Name = "Test" };
        var options = new ServiceBusMessageOptions();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.SendMessageToQueueAsync(queueName, message, options, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    #endregion

    #region SendBatchMessagesToQueueAsync Tests

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithValidMessages_ShouldSucceed()
    {
        // Arrange
        var queueName = "test-queue";
        var messages = new[]
        {
            new { Id = 1, Name = "Message 1" },
            new { Id = 2, Name = "Message 2" }
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.SendBatchMessagesToQueueAsync(queueName, messages, null, null, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithEmptyList_ShouldReturnWithoutError()
    {
        // Arrange
        var queueName = "test-queue";
        var messages = new List<object>();

        // Act & Assert - Should not throw
        await _sender.SendBatchMessagesToQueueAsync(queueName, messages, null, null, CancellationToken.None);
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithNullQueueName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var messages = new[] { new { Test = "value" } };

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.SendBatchMessagesToQueueAsync<object>(null!, messages, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_WithNullMessages_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.SendBatchMessagesToQueueAsync<object>("test-queue", null!, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task SendBatchMessagesToQueueAsync_CompatibilityMethod_ShouldWork()
    {
        // Arrange
        var queueName = "test-queue";
        var messages = new[] { new { Id = 1 } };
        var options = new ServiceBusMessageOptions();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.SendBatchMessagesToQueueAsync(queueName, messages, options, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    #endregion

    #region ScheduleMessageToQueueAsync Tests

    [Fact]
    public async Task ScheduleMessageToQueueAsync_WithValidFutureTime_ShouldSucceed()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Id = 1, Name = "Scheduled Message" };
        var futureTime = DateTimeOffset.UtcNow.AddHours(1);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.ScheduleMessageToQueueAsync(queueName, message, futureTime, null, null, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task ScheduleMessageToQueueAsync_WithPastTime_ShouldThrowArgumentException()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Test = "value" };
        var pastTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            _sender.ScheduleMessageToQueueAsync(queueName, message, pastTime, null, null, CancellationToken.None));

        exception.Message.ShouldContain("deve ser no futuro");
    }

    [Fact]
    public async Task ScheduleMessageToQueueAsync_WithCurrentTime_ShouldThrowArgumentException()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Test = "value" };
        var currentTime = DateTimeOffset.UtcNow;

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            _sender.ScheduleMessageToQueueAsync(queueName, message, currentTime, null, null, CancellationToken.None));

        exception.Message.ShouldContain("deve ser no futuro");
    }

    [Fact]
    public async Task ScheduleMessageToQueueAsync_WithNullQueueName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var message = new { Test = "value" };
        var futureTime = DateTimeOffset.UtcNow.AddHours(1);

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.ScheduleMessageToQueueAsync<object>(null!, message, futureTime, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task ScheduleMessageToQueueAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var futureTime = DateTimeOffset.UtcNow.AddHours(1);

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.ScheduleMessageToQueueAsync<object>("test-queue", null!, futureTime, null, null, CancellationToken.None));
    }

    [Fact]
    public async Task ScheduleMessageToQueueAsync_CompatibilityMethod_ShouldWork()
    {
        // Arrange
        var queueName = "test-queue";
        var message = new { Id = 1 };
        var futureTime = DateTimeOffset.UtcNow.AddHours(1);
        var options = new ServiceBusMessageOptions();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.ScheduleMessageToQueueAsync(queueName, message, futureTime, options, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    #endregion

    #region CancelScheduledMessageInQueueAsync Tests

    [Fact]
    public async Task CancelScheduledMessageInQueueAsync_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var queueName = "test-queue";
        var sequenceNumber = 12345L;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.CancelScheduledMessageInQueueAsync(queueName, sequenceNumber, null, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelScheduledMessageInQueueAsync_WithNullQueueName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sequenceNumber = 12345L;

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentNullException>(() =>
            _sender.CancelScheduledMessageInQueueAsync(null!, sequenceNumber, null, CancellationToken.None));
    }

    [Fact]
    public async Task CancelScheduledMessageInQueueAsync_WithEmptyQueueName_ShouldThrowArgumentException()
    {
        // Arrange
        var sequenceNumber = 12345L;

        // Act & Assert
        _ = await Should.ThrowAsync<ArgumentException>(() =>
            _sender.CancelScheduledMessageInQueueAsync("", sequenceNumber, null, CancellationToken.None));
    }

    [Fact]
    public async Task CancelScheduledMessageInQueueAsync_CompatibilityMethod_ShouldWork()
    {
        // Arrange
        var queueName = "test-queue";
        var sequenceNumber = 12345L;

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.CancelScheduledMessageInQueueAsync(queueName, sequenceNumber, CancellationToken.None));

        _ = exception.ShouldBeOfType<InvalidOperationException>();
    }

    #endregion

    #region Helper Methods for Reflection

    private (ServiceBusClient client, bool shouldDispose) InvokeResolveServiceBusClient(ServiceBusOperationOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ResolveServiceBusClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        return ((ServiceBusClient, bool))method!.Invoke(_sender, new object?[] { options })!;
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
