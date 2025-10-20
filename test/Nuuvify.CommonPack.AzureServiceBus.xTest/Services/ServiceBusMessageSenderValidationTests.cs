using System.Reflection;

namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderValidationTests : IClassFixture<ServiceBusTestFixture>, IAsyncDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Mock<ILogger<ServiceBusMessageSender>> _mockLogger;
    private readonly ServiceBusMessageSender _serviceBusMessageSender;

    public ServiceBusMessageSenderValidationTests(ServiceBusTestFixture fixture)
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

    #region ValidateDisposed Tests

    [Fact]
    public void ValidateDisposed_WithActiveInstance_ShouldNotThrow()
    {
        // Act & Assert - Calling any method on active instance should work
        Should.NotThrow(() =>
            InvokeValidateDisposed(_serviceBusMessageSender));
    }

    [Fact]
    public async Task ValidateDisposed_WithDisposedInstance_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);

        // Dispose the sender
        await sender.DisposeAsync();

        // Act & Assert
        var exception = Should.Throw<ObjectDisposedException>(() =>
            InvokeValidateDisposed(sender));

        exception.ObjectName.ShouldBe("ServiceBusMessageSender");
    }

    [Fact]
    public async Task ValidateDisposed_AfterMultipleDisposes_ShouldStillThrowObjectDisposedException()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);

        // Dispose multiple times (should be safe)
        await sender.DisposeAsync();
        await sender.DisposeAsync(); // Second dispose should be safe

        // Act & Assert
        var exception = Should.Throw<ObjectDisposedException>(() =>
            InvokeValidateDisposed(sender));

        exception.ObjectName.ShouldBe("ServiceBusMessageSender");
    }

    [Fact]
    public async Task ValidateDisposed_CallsOnPublicMethodsAfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);
        await sender.DisposeAsync();

        var message = new { Id = 1, Name = "Test" };
        var topicName = "test-topic";
        var queueName = "test-queue";

        // Act & Assert - All public methods should throw ObjectDisposedException
        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendMessageToQueueAsync(queueName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendMessageToTopicAsync(topicName, message, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendBatchMessagesToQueueAsync(queueName, new[] { message }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.SendBatchMessagesToTopicAsync(topicName, new[] { message }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.ScheduleMessageToQueueAsync(queueName, message, DateTimeOffset.UtcNow.AddMinutes(30), messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.ScheduleMessageToTopicAsync(topicName, message, DateTimeOffset.UtcNow.AddMinutes(30), messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.CancelScheduledMessageInQueueAsync(queueName, 12345L, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await sender.CancelScheduledMessageInTopicAsync(topicName, 12345L, operationOptions: null, cancellationToken: CancellationToken.None));
    }

    #endregion

    #region ValidateParameter Tests

    [Fact]
    public void ValidateParameter_WithValidString_ShouldNotThrow()
    {
        // Arrange
        var validString = "valid-parameter";

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validString, "testParameter"));
    }

    [Fact]
    public void ValidateParameter_WithNullString_ShouldThrowArgumentNullException()
    {
        // Arrange
        string nullString = null!;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(nullString, "testParameter"));

        exception.ParamName.ShouldBe("testParameter");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t \n  ")]
    public void ValidateParameter_WithEmptyOrWhitespaceString_ShouldThrowArgumentException(string invalidString)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            InvokeValidateParameter(invalidString, "testParameter"));

        exception.ParamName.ShouldBe("testParameter");
        exception.Message.ShouldContain("testParameter não pode ser nulo ou vazio");
    }

    [Fact]
    public void ValidateParameter_WithValidObject_ShouldNotThrow()
    {
        // Arrange
        var validObject = new { Id = 1, Name = "Test" };

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validObject, "testObject"));
    }

    [Fact]
    public void ValidateParameter_WithNullObject_ShouldThrowArgumentNullException()
    {
        // Arrange
        object nullObject = null!;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(nullObject, "testObject"));

        exception.ParamName.ShouldBe("testObject");
    }

    [Fact]
    public void ValidateParameter_WithValidInt_ShouldNotThrow()
    {
        // Arrange
        var validInt = 42;

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validInt, "testInt"));
    }

    [Fact]
    public void ValidateParameter_WithDefaultInt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var defaultInt = default(int); // 0

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(defaultInt, "testInt"));

        exception.ParamName.ShouldBe("testInt");
    }

    [Fact]
    public void ValidateParameter_WithValidGuid_ShouldNotThrow()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validGuid, "testGuid"));
    }

    [Fact]
    public void ValidateParameter_WithEmptyGuid_ShouldThrowArgumentNullException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(emptyGuid, "testGuid"));

        exception.ParamName.ShouldBe("testGuid");
    }

    [Fact]
    public void ValidateParameter_WithValidDateTime_ShouldNotThrow()
    {
        // Arrange
        var validDateTime = DateTime.UtcNow;

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validDateTime, "testDateTime"));
    }

    [Fact]
    public void ValidateParameter_WithDefaultDateTime_ShouldThrowArgumentNullException()
    {
        // Arrange
        var defaultDateTime = default(DateTime);

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(defaultDateTime, "testDateTime"));

        exception.ParamName.ShouldBe("testDateTime");
    }

    [Fact]
    public void ValidateParameter_WithValidNullableInt_ShouldNotThrow()
    {
        // Arrange
        int? validNullableInt = 42;

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validNullableInt, "testNullableInt"));
    }

    [Fact]
    public void ValidateParameter_WithNullNullableInt_ShouldThrowArgumentNullException()
    {
        // Arrange
        int? nullNullableInt = null;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(nullNullableInt, "testNullableInt"));

        exception.ParamName.ShouldBe("testNullableInt");
    }

    [Fact]
    public void ValidateParameter_WithValidCollection_ShouldNotThrow()
    {
        // Arrange
        var validCollection = new List<string> { "item1", "item2" };

        // Act & Assert
        Should.NotThrow(() =>
            InvokeValidateParameter(validCollection, "testCollection"));
    }

    [Fact]
    public void ValidateParameter_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        List<string> nullCollection = null!;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(nullCollection, "testCollection"));

        exception.ParamName.ShouldBe("testCollection");
    }

    [Fact]
    public void ValidateParameter_WithEmptyCollection_ShouldNotThrow()
    {
        // Arrange
        var emptyCollection = new List<string>();

        // Act & Assert - Empty collection is valid (not default/null)
        Should.NotThrow(() =>
            InvokeValidateParameter(emptyCollection, "testCollection"));
    }

    [Theory]
    [InlineData("queueName")]
    [InlineData("topicName")]
    [InlineData("messageOptions")]
    [InlineData("operationOptions")]
    [InlineData("messages")]
    [InlineData("message")]
    [InlineData("connectionString")]
    [InlineData("clientOptions")]
    public void ValidateParameter_WithDifferentParameterNames_ShouldIncludeCorrectParameterName(string parameterName)
    {
        // Arrange
        string nullValue = null!;

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            InvokeValidateParameter(nullValue, parameterName));

        exception.ParamName.ShouldBe(parameterName);
    }

    #endregion

    #region Integration Tests with Validation

    [Fact]
    public async Task QueueMethods_WithInvalidParameters_ShouldValidateCorrectly()
    {
        // Act & Assert - All queue methods should validate parameters
        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToQueueAsync("", new { Id = 1 }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToQueueAsync<object>("valid-queue", null!, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToQueueAsync("", new[] { new { Id = 1 } }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToQueueAsync<object>("valid-queue", null!, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToQueueAsync("", new { Id = 1 }, DateTimeOffset.UtcNow.AddMinutes(30), messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInQueueAsync("", 12345L, operationOptions: null, cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task TopicMethods_WithInvalidParameters_ShouldValidateCorrectly()
    {
        // Act & Assert - All topic methods should validate parameters
        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync("", new { Id = 1 }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendMessageToTopicAsync<object>("valid-topic", null!, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync("", new[] { new { Id = 1 } }, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.SendBatchMessagesToTopicAsync<object>("valid-topic", null!, messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.ScheduleMessageToTopicAsync("", new { Id = 1 }, DateTimeOffset.UtcNow.AddMinutes(30), messageOptions: null, operationOptions: null, cancellationToken: CancellationToken.None));

        _ = await Should.ThrowAsync<ArgumentException>(async () =>
            await _serviceBusMessageSender.CancelScheduledMessageInTopicAsync("", 12345L, operationOptions: null, cancellationToken: CancellationToken.None));
    }

    #endregion

    #region Performance and Edge Cases

    [Fact]
    public void ValidateParameter_WithLargeString_ShouldPerformWell()
    {
        // Arrange
        var largeString = new string('A', 10000);
        var startTime = DateTime.UtcNow;

        // Act
        Should.NotThrow(() =>
            InvokeValidateParameter(largeString, "largeString"));

        // Assert
        var duration = DateTime.UtcNow - startTime;
        duration.ShouldBeLessThan(TimeSpan.FromMilliseconds(100)); // Should be very fast
    }

    [Fact]
    public void ValidateParameter_WithManyValidations_ShouldPerformWell()
    {
        // Arrange
        var validString = "valid-parameter";
        var startTime = DateTime.UtcNow;

        // Act - Perform many validations
        for (int i = 0; i < 1000; i++)
        {
            InvokeValidateParameter(validString, $"parameter{i}");
        }

        // Assert
        var duration = DateTime.UtcNow - startTime;
        duration.ShouldBeLessThan(TimeSpan.FromMilliseconds(500)); // Should be fast
    }

    [Fact]
    public void ValidateParameter_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange & Act & Assert - Special characters should be valid
        Should.NotThrow(() =>
            InvokeValidateParameter("test-queue_123", "queueName"));

        Should.NotThrow(() =>
            InvokeValidateParameter("test.topic-name", "topicName"));

        Should.NotThrow(() =>
            InvokeValidateParameter("special!@#$%^&*()characters", "parameterName"));

        Should.NotThrow(() =>
            InvokeValidateParameter("unicode-テスト-測試", "unicodeParameter"));
    }

    [Fact]
    public void ValidateParameter_WithTypeBoundaries_ShouldHandleCorrectly()
    {
        // Act & Assert - Test boundary values for different types
        Should.NotThrow(() =>
            InvokeValidateParameter(int.MinValue, "minInt"));

        Should.NotThrow(() =>
            InvokeValidateParameter(int.MaxValue, "maxInt"));

        Should.NotThrow(() =>
            InvokeValidateParameter(DateTime.MinValue.AddTicks(1), "almostMinDateTime")); // Avoid default

        Should.NotThrow(() =>
            InvokeValidateParameter(DateTime.MaxValue, "maxDateTime"));

        Should.NotThrow(() =>
            InvokeValidateParameter(long.MinValue, "minLong"));

        Should.NotThrow(() =>
            InvokeValidateParameter(long.MaxValue, "maxLong"));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Uses reflection to invoke the private ValidateDisposed method
    /// </summary>
    private static void InvokeValidateDisposed(ServiceBusMessageSender sender)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ValidateDisposed",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method is null)
        {
            throw new InvalidOperationException("Could not find ValidateDisposed method");
        }

        try
        {
            _ = method.Invoke(sender, null);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    /// <summary>
    /// Uses reflection to invoke the private ValidateParameter method
    /// </summary>
    private static void InvokeValidateParameter<T>(T parameter, string parameterName)
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

    #endregion
}
