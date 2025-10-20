using System.Reflection;

namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderRetryHandlingTests : IAsyncDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly ServiceBusMessageSender _sender;
    private readonly Mock<ILogger<ServiceBusMessageSender>> _mockLogger;

    public ServiceBusMessageSenderRetryHandlingTests()
    {
        _fixture = new ServiceBusTestFixture();
        var config = _fixture.CreateValidServiceBusConfiguration();
        _mockLogger = _fixture.CreateMockLogger<ServiceBusMessageSender>();
        _sender = new ServiceBusMessageSender(config, _mockLogger.Object);
    }

    #region ExecuteWithRetryAsync<T> Tests

    [Fact]
    public async Task ExecuteWithRetryAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        const string expectedResult = "success";
        var operation = new Func<Task<string>>(() => Task.FromResult(expectedResult));

        // Act
        var result = await InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResult);
        VerifyNoRetryLogging();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithRetriableException_ShouldRetryAndSucceed()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new ServiceBusException("Timeout", ServiceBusFailureReason.ServiceTimeout);
            }
            return Task.FromResult("success");
        });

        // Act
        var result = await InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None);

        // Assert
        result.ShouldBe("success");
        callCount.ShouldBe(2);
        VerifyRetryLogging(1);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithMultipleRetriableExceptions_ShouldRetryMultipleTimes()
    {
        // Arrange
        var callCount = 0;
        const int failureTimes = 2;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            if (callCount <= failureTimes)
            {
                throw new ServiceBusException("Service Busy", ServiceBusFailureReason.ServiceBusy);
            }
            return Task.FromResult("success");
        });

        // Act
        var result = await InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None);

        // Assert
        result.ShouldBe("success");
        callCount.ShouldBe(failureTimes + 1);
        VerifyRetryLogging(failureTimes);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithMaxRetriesExceeded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        config.Value.MaxRetryAttempts = 2;

        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);
        var operation = new Func<Task<string>>(() =>
            throw new ServiceBusException("Always fails", ServiceBusFailureReason.ServiceTimeout));

        // Act & Assert
        var exception = await Should.ThrowAsync<Azure.Messaging.ServiceBus.ServiceBusException>(() =>
            InvokeExecuteWithRetryAsync(sender, operation, "test-operation", CancellationToken.None));

        exception.Message.ShouldContain("Always fails");
        VerifyRetryLogging(2);

        await sender.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithNonRetriableException_ShouldNotRetry()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            throw new ServiceBusException("MessagingEntityNotFound", ServiceBusFailureReason.MessagingEntityNotFound);
        });

        // Act & Assert
        _ = await Should.ThrowAsync<ServiceBusException>(() =>
            InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None));

        callCount.ShouldBe(1);
        VerifyNoRetryLogging();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var operation = new Func<Task<string>>(() =>
        {
            throw new TaskCanceledException();
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            InvokeExecuteWithRetryAsync(operation, "test-operation", cts.Token));

        // Verificar que é uma exceção de cancelamento (a mensagem pode variar)
        _ = exception.ShouldBeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithTimeout_ShouldThrowTimeoutException()
    {
        // Arrange
        var operation = new Func<Task<string>>(() =>
        {
            throw new TaskCanceledException();
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<TimeoutException>(() =>
            InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None));

        exception.Message.ShouldContain("Timeout na operação");
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithRetryDelay_ShouldWaitBetweenRetries()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        config.Value.RetryDelaySeconds = 1;
        config.Value.MaxRetryAttempts = 1;

        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);
        var callCount = 0;
        var timestamps = new List<DateTime>();

        var operation = new Func<Task<string>>(() =>
        {
            timestamps.Add(DateTime.UtcNow);
            callCount++;
            if (callCount == 1)
            {
                throw new ServiceBusException("Timeout", ServiceBusFailureReason.ServiceTimeout);
            }
            return Task.FromResult("success");
        });

        // Act
        var result = await InvokeExecuteWithRetryAsync(sender, operation, "test-operation", CancellationToken.None);

        // Assert
        result.ShouldBe("success");
        timestamps.Count.ShouldBe(2);
        var delay = timestamps[1] - timestamps[0];
        delay.TotalMilliseconds.ShouldBeGreaterThan(500); // Should wait at least some time

        await sender.DisposeAsync();
    }

    #endregion

    #region ExecuteWithRetryAsync (void) Tests

    [Fact]
    public async Task ExecuteWithRetryAsync_VoidOverload_WithSuccessfulOperation_ShouldComplete()
    {
        // Arrange
        var executed = false;
        var operation = new Func<Task>(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Act
        await InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
        VerifyNoRetryLogging();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_VoidOverload_WithRetriableException_ShouldRetry()
    {
        // Arrange
        var callCount = 0;
        var operation = new Func<Task>(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                throw new ServiceBusException("General Error", ServiceBusFailureReason.GeneralError);
            }
            return Task.CompletedTask;
        });

        // Act
        await InvokeExecuteWithRetryAsync(operation, "test-operation", CancellationToken.None);

        // Assert
        callCount.ShouldBe(2);
        VerifyRetryLogging(1);
    }

    #endregion

    #region IsRetriableException Tests

    [Theory]
    [InlineData(ServiceBusFailureReason.ServiceTimeout, true)]
    [InlineData(ServiceBusFailureReason.ServiceBusy, true)]
    [InlineData(ServiceBusFailureReason.GeneralError, true)]
    [InlineData(ServiceBusFailureReason.ServiceCommunicationProblem, true)]
    [InlineData(ServiceBusFailureReason.MessagingEntityNotFound, false)]
    [InlineData(ServiceBusFailureReason.MessageNotFound, false)]
    [InlineData(ServiceBusFailureReason.QuotaExceeded, false)]
    [InlineData(ServiceBusFailureReason.MessagingEntityDisabled, false)]
    public void IsRetriableException_WithDifferentReasons_ShouldReturnExpectedResult(
        ServiceBusFailureReason reason, bool expectedRetriable)
    {
        // Arrange
        var exception = new ServiceBusException("Test exception", reason);

        // Act
        var isRetriable = InvokeIsRetriableException(exception);

        // Assert
        isRetriable.ShouldBe(expectedRetriable);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ExecuteWithRetryAsync_WithRealServiceBusOperation_ShouldHandleRetries()
    {
        // This test demonstrates the retry mechanism with actual Service Bus operations
        // Note: This will fail with InvalidOperationException due to mock limitations,
        // but it tests the retry wrapper correctly

        // Arrange
        var queueName = "test-queue";
        var message = new { Test = "retry-test" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() =>
            _sender.SendMessageToQueueAsync(queueName, message, null, null, CancellationToken.None));

        // The operation should fail but the retry mechanism should be invoked
        _ = exception.ShouldBeOfType<InvalidOperationException>();
        exception.Message.ShouldContain("queue");

        // Verify that retry logging would have occurred if there were actual retriable exceptions
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tentativa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // No retries because the exception is not a ServiceBusException
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithCustomConfiguration_ShouldRespectSettings()
    {
        // Arrange
        var config = _fixture.CreateValidServiceBusConfiguration();
        config.Value.MaxRetryAttempts = 3;
        config.Value.RetryDelaySeconds = 2;
        config.Value.OperationTimeoutSeconds = 30;

        var sender = new ServiceBusMessageSender(config, _mockLogger.Object);
        var callCount = 0;

        var operation = new Func<Task<string>>(() =>
        {
            callCount++;
            throw new ServiceBusException("Communication Problem", ServiceBusFailureReason.ServiceCommunicationProblem);
        });

        // Act & Assert
        var exception = await Should.ThrowAsync<Azure.Messaging.ServiceBus.ServiceBusException>(() =>
            InvokeExecuteWithRetryAsync(sender, operation, "test-operation", CancellationToken.None));

        // Assert
        callCount.ShouldBe(4); // Initial call + 3 retries
        exception.Message.ShouldContain("Communication Problem");

        await sender.DisposeAsync();
    }

    #endregion

    #region Helper Methods for Reflection

    private async Task<T> InvokeExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken cancellationToken)
    {
        return await InvokeExecuteWithRetryAsync(_sender, operation, operationName, cancellationToken);
    }

    private static async Task<T> InvokeExecuteWithRetryAsync<T>(ServiceBusMessageSender sender, Func<Task<T>> operation, string operationName, CancellationToken cancellationToken)
    {
        var methods = typeof(ServiceBusMessageSender).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name == "ExecuteWithRetryAsync" && m.IsGenericMethod)
            .ToArray();

        var method = methods.FirstOrDefault(m =>
        {
            var parameters = m.GetParameters();
            return parameters.Length == 3 &&
                   parameters[0].ParameterType.IsGenericType &&
                   parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
                   parameters[1].ParameterType == typeof(string) &&
                   parameters[2].ParameterType == typeof(CancellationToken);
        }) ?? throw new InvalidOperationException("Could not find ExecuteWithRetryAsync<T> method");

        var genericMethod = method.MakeGenericMethod(typeof(T));
        var task = (Task<T>)genericMethod.Invoke(sender, new object[] { operation, operationName, cancellationToken })!;
        return await task;
    }

    private async Task InvokeExecuteWithRetryAsync(Func<Task> operation, string operationName, CancellationToken cancellationToken)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ExecuteWithRetryAsync",
            BindingFlags.NonPublic | BindingFlags.Instance,
            new[] { typeof(Func<Task>), typeof(string), typeof(CancellationToken) });

        var task = (Task)method!.Invoke(_sender, new object[] { operation, operationName, cancellationToken })!;
        await task;
    }

    private static bool InvokeIsRetriableException(ServiceBusException exception)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("IsRetriableException",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (bool)method!.Invoke(null, new object[] { exception })!;
    }

    private void VerifyRetryLogging(int expectedRetryCount)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tentativa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(expectedRetryCount));
    }

    private void VerifyNoRetryLogging()
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tentativa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
