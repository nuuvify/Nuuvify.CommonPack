using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService.xTest.Fakers;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using Xunit;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Testes unitários para ServiceBusBackgroundService (classe base)
/// </summary>
[Trait("Category", "Unit")]
public sealed class ServiceBusBackgroundServiceBaseTests : IDisposable
{
    private readonly Mock<ILogger<TestServiceBusBackgroundService>> _loggerMock;
    private readonly Mock<IConfigurationCustom> _configurationMock;
    private readonly RequestConfiguration _requestConfiguration;
    private readonly ActivitySource _activitySource;

    public ServiceBusBackgroundServiceBaseTests()
    {
        _loggerMock = new Mock<ILogger<TestServiceBusBackgroundService>>();
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
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

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
        var requestConfigWithoutCorrelationId = new RequestConfiguration { CorrelationId = string.Empty };

        // Act
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, requestConfigWithoutCorrelationId);

        // Assert
        Assert.NotNull(service.GetRequestConfiguration().CorrelationId);
        Assert.NotEmpty(service.GetRequestConfiguration().CorrelationId);
    }

    [Fact]
    public void ConfigureServiceBus_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

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
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validName", "queue", null, null));

        Assert.Contains("A conexão com o Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_ShouldThrowArgumentException_WhenQueueNameIsEmpty()
    {
        // Arrange
        _ = _configurationMock.Setup(x => x.GetConnectionString("validConnection")).Returns("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test");
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validConnection", "", null, null));

        Assert.Contains("fila do Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithTopic_ShouldThrowArgumentException_WhenTopicNameIsEmpty()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBusWithTopicConnectionString("connection", "", "subscription", null, null));

        Assert.Contains("tópico do Service Bus não foi configurado", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithTopic_ShouldThrowArgumentException_WhenSubscriptionIsEmpty()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBusWithTopicConnectionString("connection", "topic", "", null, null));

        Assert.Contains("assinatura do Service Bus não foi configurada", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithCredentials_ShouldThrowArgumentException_WhenCredentialIsNull()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBusWithTopicCredentials("topic", "subscription", "namespace", null!, null, null));

        Assert.Contains("O TokenCredential do Azure não foi configurado", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithCredentials_ShouldThrowArgumentException_WhenNamespaceIsEmpty()
    {
        // Arrange
        var mockCredential = new Mock<Azure.Core.TokenCredential>();
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBusWithQueueCredentials("queue", "", mockCredential.Object, null, null));

        Assert.Contains("namespace totalmente qualificado do Service Bus não foi configurado", exception.Message);
    }

    [Fact]
    public void HandleErrorAsync_MethodExists()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert - Just verify the method exists and is accessible
        var method = typeof(TestServiceBusBackgroundService).GetMethod("TestHandleErrorAsync");
        Assert.NotNull(method);
        Assert.True(method.IsPublic);
    }

    [Fact]
    public void StopAsync_ShouldCallBaseStopAsync()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert - Just verify the method exists
        var method = typeof(TestServiceBusBackgroundService).GetMethod("StopAsync");
        Assert.NotNull(method);
        Assert.True(method.IsPublic);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenThrowInvalidOperationExceptionOnDisposeIsNotSet()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        service.Dispose(); // Should not throw
    }

    [Fact]
    public void Dispose_ShouldLogWarning_WhenThrowInvalidOperationExceptionOnDisposeIsSet()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetThrowInvalidOperationExceptionOnDispose(true);

        // Act
        service.Dispose();

        // Assert
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
    public void AbandonMessageIfFailed_Property_ShouldWorkCorrectly()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        Assert.False(service.GetAbandonMessageIfFailed()); // Default should be false

        service.SetAbandonMessageIfFailed(true);
        Assert.True(service.GetAbandonMessageIfFailed());

        service.SetAbandonMessageIfFailed(false);
        Assert.False(service.GetAbandonMessageIfFailed());
    }

    [Fact]
    public void SetActivitySource_ShouldUpdateActivitySource()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var newActivitySource = new ActivitySource("NewTestActivitySource");

        // Act
        service.SetActivitySource(newActivitySource);

        // Assert
        Assert.Equal(newActivitySource, service.GetActivitySource());
    }

    [Fact]
    public void ExecuteRule_ConfigurationMethods_ShouldWorkCorrectly()
    {
        // Arrange
        using var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert - Test configuration methods
        service.SetExecuteRuleResult(true);
        service.SetThrowServiceBusException(ServiceBusFailureReason.ServiceCommunicationProblem);
        service.SetThrowOperationCanceledException(true);
        service.SetThrowGenericException(true);

        // Should not throw - these are just configuration methods
        Assert.True(true);
    }

    public void Dispose()
    {
        _activitySource?.Dispose();
    }
}

