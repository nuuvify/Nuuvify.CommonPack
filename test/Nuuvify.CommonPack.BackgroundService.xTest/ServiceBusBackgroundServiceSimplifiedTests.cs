using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService.xTest.Fakers;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using Xunit;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Testes unitários simplificados para ServiceBusBackgroundService
/// Evita problemas com classes sealed do Azure SDK
/// </summary>
[Trait("Category", "Unit")]
public sealed class ServiceBusBackgroundServiceSimplifiedTests : IDisposable
{
    private readonly Mock<ILogger<TestServiceBusBackgroundServiceSimplified>> _loggerMock;
    private readonly Mock<IConfigurationCustom> _configurationMock;
    private readonly RequestConfiguration _requestConfiguration;
    private readonly ActivitySource _activitySource;

    public ServiceBusBackgroundServiceSimplifiedTests()
    {
        _loggerMock = new Mock<ILogger<TestServiceBusBackgroundServiceSimplified>>();
        _configurationMock = ServiceBusBackgroundServiceFaker.GenerateConfigurationMock();
        _requestConfiguration = new RequestConfiguration
        {
            CorrelationId = Guid.NewGuid().ToString()
        };
        _activitySource = new ActivitySource("TestActivitySource");
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Assert
        Assert.NotNull(service.GetLogger());
        Assert.NotNull(service.GetConfigurationCustom());
        Assert.NotNull(service.GetRequestConfiguration());
        Assert.Equal(_requestConfiguration.CorrelationId, service.GetRequestConfiguration().CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldGenerateCorrelationId_WhenNotProvided()
    {
        // Arrange
        var emptyRequestConfiguration = new RequestConfiguration { CorrelationId = null };

        // Act
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, emptyRequestConfiguration);

        // Assert
        Assert.NotNull(service.GetRequestConfiguration().CorrelationId);
        Assert.NotEmpty(service.GetRequestConfiguration().CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldHandleNullLogger()
    {
        // Act & Assert - The constructor will accept null logger
        using var service = new TestServiceBusBackgroundServiceSimplified(null!, _configurationMock.Object, _requestConfiguration);

        // Should not throw, but logger will be null
        Assert.Null(service.GetLogger());
    }

    [Fact]
    public void Constructor_ShouldHandleNullConfiguration()
    {
        // Act & Assert - The constructor will accept null configuration
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, null!, _requestConfiguration);

        // Should not throw, but configuration will be null
        Assert.Null(service.GetConfigurationCustom());
    }

    [Fact]
    public void Constructor_ShouldThrowNullReferenceException_WhenRequestConfigurationIsNull()
    {
        // Act & Assert - Will throw NullReferenceException when trying to access CorrelationId
        var exception = Assert.Throws<NullReferenceException>(() =>
            new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, null!));
    }

    [Fact]
    public void ConfigureServiceBus_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("", "queue", null, null));

        Assert.Contains("Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithConnectionString_ShouldThrowArgumentException_WhenConnectionStringIsNull()
    {
        // Arrange
        _ = _configurationMock.Setup(x => x.GetConnectionString("validName")).Returns(string.Empty);
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validName", "queue", null, null));

        Assert.Contains("Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_ShouldThrowArgumentException_WhenQueueNameIsEmpty()
    {
        // Arrange
        _ = _configurationMock.Setup(x => x.GetConnectionString("validConnection")).Returns("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test");
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validConnection", "", null, null));

        Assert.Contains("Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void SetActivitySource_ShouldUpdateActivitySource()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var newActivitySource = new ActivitySource("NewTestActivitySource");

        // Act
        service.SetActivitySource(newActivitySource);

        // Assert
        Assert.Equal(newActivitySource, service.GetActivitySource());

        // Cleanup
        newActivitySource.Dispose();
    }

    [Fact]
    public void AbandonMessageIfFailed_Property_ShouldWorkCorrectly()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert - Default value should be false
        Assert.False(service.GetAbandonMessageIfFailed());

        // Act - Set to true
        service.SetAbandonMessageIfFailed(true);

        // Assert
        Assert.True(service.GetAbandonMessageIfFailed());

        // Act - Set to false
        service.SetAbandonMessageIfFailed(false);

        // Assert
        Assert.False(service.GetAbandonMessageIfFailed());
    }

    [Fact]
    public void ExecuteRule_ConfigurationMethods_ShouldWorkCorrectly()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Test SetExecuteRuleResult
        service.SetExecuteRuleResult(false);
        Assert.False(service.TestExecuteRuleResult);

        service.SetExecuteRuleResult(true);
        Assert.True(service.TestExecuteRuleResult);

        // Test exception configurations
        service.SetThrowOperationCanceledException(true);
        Assert.True(service.TestThrowOperationCanceledException);

        service.SetThrowGenericException(true);
        Assert.True(service.TestThrowGenericException);

        service.SetThrowInvalidOperationExceptionOnDispose(true);
        Assert.True(service.TestThrowOnDispose);
    }

    [Fact]
    public void Dispose_ShouldLogWarning_WhenThrowInvalidOperationExceptionOnDisposeIsSet()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetThrowInvalidOperationExceptionOnDispose(true);

        // Act
        service.Dispose();

        // Assert - Verify that warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Recurso do Service Bus já estava em processo de liberação")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenThrowInvalidOperationExceptionOnDisposeIsNotSet()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundServiceSimplified(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert - Should not throw
        service.Dispose();
    }

    public void Dispose()
    {
        _activitySource.Dispose();
        GC.SuppressFinalize(this);
    }
}

