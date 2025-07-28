using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using Xunit;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

public sealed class BackgroundServiceAbstractTests : IDisposable
{
    private readonly Mock<ILogger<TestBackgroundService>> _mockLogger;
    private readonly Mock<IConfigurationCustom> _mockConfiguration;
    private readonly RequestConfiguration _requestConfiguration;
    private readonly ActivitySource _activitySource;
    private readonly TestBackgroundService _backgroundService;

    public BackgroundServiceAbstractTests()
    {
        _mockLogger = new Mock<ILogger<TestBackgroundService>>();
        _mockConfiguration = new Mock<IConfigurationCustom>();
        _requestConfiguration = new RequestConfiguration
        {
            CorrelationId = Guid.NewGuid().ToString()
        };
        _activitySource = new ActivitySource("TestActivitySource");

        // Setup mock configuration
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Cnn"))
            .Returns("TestConnection");
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Topic"))
            .Returns("test-topic");
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Subscription"))
            .Returns("test-subscription");
        _ = _mockConfiguration.Setup(x => x.GetConnectionString("TestConnection"))
            .Returns("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=testkey");

        _backgroundService = new TestBackgroundService(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _requestConfiguration,
            _activitySource);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldInitializeAllPropertiesCorrectly()
    {
        // Assert - Validating all properties individually in a single method
        Assert.Same(_mockLogger.Object, _backgroundService.Logger);
        Assert.Same(_mockConfiguration.Object, _backgroundService.ConfigurationCustom);
        Assert.Same(_requestConfiguration, _backgroundService.RequestConfiguration);
        Assert.Same(_activitySource, _backgroundService.ActivitySourceCustom);
        Assert.False(_backgroundService.AbandonMessageIfFailed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldGenerateCorrelationId_WhenNotProvided()
    {
        // Arrange
        var requestConfig = new RequestConfiguration();

        // Act
        using var service = new TestBackgroundService(
            Mock.Of<ILogger<TestBackgroundService>>(),
            _mockConfiguration.Object,
            requestConfig,
            _activitySource);

        // Assert
        Assert.False(string.IsNullOrEmpty(service.RequestConfiguration.CorrelationId));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        // Arrange
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Cnn"))
            .Returns(string.Empty);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            using var service = new TestBackgroundService(
                Mock.Of<ILogger<TestBackgroundService>>(),
                _mockConfiguration.Object,
                _requestConfiguration,
                _activitySource);
        });

        Assert.Contains("A conexão com o Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldThrowArgumentException_WhenTopicIsEmpty()
    {
        // Arrange
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Topic"))
            .Returns(string.Empty);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            using var service = new TestBackgroundService(
                Mock.Of<ILogger<TestBackgroundService>>(),
                _mockConfiguration.Object,
                _requestConfiguration,
                _activitySource);
        });

        Assert.Contains("O tópico do Service Bus não foi configurado corretamente", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_ShouldThrowArgumentException_WhenSubscriptionIsEmpty()
    {
        // Arrange
        _ = _mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Subscription"))
            .Returns(string.Empty);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            using var service = new TestBackgroundService(
                Mock.Of<ILogger<TestBackgroundService>>(),
                _mockConfiguration.Object,
                _requestConfiguration,
                _activitySource);
        });

        Assert.Contains("A assinatura do Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteRule_ShouldReturnTrue_WhenImplementedCorrectly()
    {
        // Arrange
        var mockMessage = Mock.Of<ServiceBusReceivedMessage>();
        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = await _backgroundService.ExecuteRule(mockMessage, _activitySource, cancellationTokenSource.Token);

        // Assert
        Assert.True(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteRule_ShouldReturnFalse_WhenProcessingFails()
    {
        // Arrange
        var mockMessage = Mock.Of<ServiceBusReceivedMessage>();
        using var cancellationTokenSource = new CancellationTokenSource();
        using var failingService = new FailingTestBackgroundService(
            Mock.Of<ILogger<FailingTestBackgroundService>>(),
            _mockConfiguration.Object,
            _requestConfiguration,
            _activitySource,
            1,
            TimeSpan.FromMinutes(5));

        // Act
        var result = await failingService.ExecuteRule(mockMessage, _activitySource, cancellationTokenSource.Token);

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AbandonMessageIfFailed_ShouldBeSettable()
    {
        // Act
        _backgroundService.AbandonMessageIfFailed = true;

        // Assert
        Assert.True(_backgroundService.AbandonMessageIfFailed);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _backgroundService?.Dispose();
            _activitySource?.Dispose();
        }
    }
}

// Classe de teste concreta para testar a classe abstrata
public class TestBackgroundService : BackgroundServiceAbstract<TestBackgroundService>
{
    public TestBackgroundService(
        ILogger<TestBackgroundService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        ActivitySource activitySourceCustom)
        : base(logger, configurationCustom, requestConfiguration, activitySourceCustom)
    {
    }

    public override Task<bool> ExecuteRule(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        // Implementação de teste simples
        return Task.FromResult(true);
    }

    // Exposing protected members for testing
    public new ILogger Logger => base.Logger;
    public new IConfigurationCustom ConfigurationCustom => base.ConfigurationCustom;
    public new RequestConfiguration RequestConfiguration => base.RequestConfiguration;
    public new ActivitySource ActivitySourceCustom => base.ActivitySourceCustom;
}

// Classe de teste que simula falha no processamento
public class FailingTestBackgroundService : BackgroundServiceAbstract<FailingTestBackgroundService>
{
    public FailingTestBackgroundService(
        ILogger<FailingTestBackgroundService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        ActivitySource activitySourceCustom,
        int maxConcurrentCalls,
        TimeSpan maxAutoLockRenewalDuration)
        : base(logger, configurationCustom, requestConfiguration, activitySourceCustom)
    {
    }

    public override Task<bool> ExecuteRule(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        // Simula falha no processamento
        return Task.FromResult(false);
    }
}
