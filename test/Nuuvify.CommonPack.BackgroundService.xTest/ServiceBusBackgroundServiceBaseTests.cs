using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.BackgroundService.Abstraction;
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
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

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
        var requestConfigWithoutCorrelationId = new RequestConfiguration();

        // Act
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, requestConfigWithoutCorrelationId);

        // Assert
        Assert.NotNull(service.GetRequestConfiguration().CorrelationId);
        Assert.NotEmpty(service.GetRequestConfiguration().CorrelationId);
    }

    [Fact]
    public void Constructor_ShouldKeepExistingCorrelationId_WhenProvided()
    {
        // Arrange
        var expectedCorrelationId = Guid.NewGuid().ToString();
        var requestConfigWithCorrelationId = new RequestConfiguration
        {
            CorrelationId = expectedCorrelationId
        };

        // Act
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, requestConfigWithCorrelationId);

        // Assert
        Assert.Equal(expectedCorrelationId, service.GetRequestConfiguration().CorrelationId);
    }

    [Fact]
    public void ConfigureServiceBus_WithConnectionString_ShouldThrowArgumentException_WhenConnectionNameIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("", "queue", null, null));

        Assert.Contains("Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithConnectionString_ShouldThrowArgumentException_WhenConnectionStringIsNull()
    {
        // Arrange
        _configurationMock.Setup(x => x.GetConnectionString("validName")).Returns(string.Empty);
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validName", "queue", null, null));

        Assert.Contains("não foi possível obter a ConnectionString do Vault", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithQueue_ShouldThrowArgumentException_WhenQueueNameIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validConnection", "", null, null));

        Assert.Contains("fila do Service Bus não foi configurada corretamente", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithConnectionString_ShouldThrowArgumentException_WhenTopicNameIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validConnection", "", "subscription", null, null, null));

        Assert.Contains("tópico do Service Bus não foi configurado", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithConnectionString_ShouldThrowArgumentException_WhenSubscriptionIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("validConnection", "topic", "", null, null, null));

        Assert.Contains("assinatura do Service Bus não foi configurada", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithCredentials_ShouldThrowArgumentException_WhenCredentialIsNull()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("queue", "namespace", null, null, null));

        Assert.Contains("TokenCredential do Azure não foi configurado", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithCredentials_ShouldThrowArgumentException_WhenTopicNameIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var mockCredential = ServiceBusBackgroundServiceFaker.GenerateTokenCredentialMock();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("", "subscription", "namespace", mockCredential.Object, null, null));

        Assert.Contains("tópico do Service Bus não foi configurado", exception.Message);
    }

    [Fact]
    public void ConfigureServiceBus_WithQueueAndCredentials_ShouldThrowArgumentException_WhenNamespaceIsEmpty()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var mockCredential = ServiceBusBackgroundServiceFaker.GenerateTokenCredentialMock();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            service.TestConfigureServiceBus("queue", "", mockCredential.Object, null, null));

        Assert.Contains("namespace totalmente qualificado do Service Bus não foi configurado", exception.Message);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldCompleteMessage_WhenExecuteRuleReturnsTrue()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(true);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        mockArgs.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        mockArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
        mockArgs.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldAbandonMessage_WhenExecuteRuleReturnsFalseAndAbandonIsTrue()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(false);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object, abandon: true);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        mockArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        mockArgs.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        mockArgs.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldDeadLetterMessage_WhenExecuteRuleReturnsFalseAndAbandonIsFalse()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetExecuteRuleResult(false);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object, abandon: false);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        mockArgs.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        mockArgs.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        mockArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldThrowArgumentException_WhenActivitySourceIsNull()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(null); // Set to null to trigger exception

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None));
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleServiceBusException_MessageLockLost()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowServiceBusException(ServiceBusFailureReason.MessageLockLost);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MessageLockLost")),
                It.IsAny<ServiceBusException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleServiceBusException_ServiceCommunicationProblem()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowServiceBusException(ServiceBusFailureReason.ServiceCommunicationProblem);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ServiceCommunicationProblem")),
                It.IsAny<ServiceBusException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleServiceBusException_QuotaExceeded()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowServiceBusException(ServiceBusFailureReason.QuotaExceeded);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("QuotaExceeded")),
                It.IsAny<ServiceBusException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleOperationCanceledException()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowOperationCanceledException(true);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object, abandon: true);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("foi cancelada")),
                It.IsAny<OperationCanceledException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleOperationCanceledException_WhenAbandonIsFalse()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowOperationCanceledException(true);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object, abandon: false);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("foi cancelada")),
                It.IsAny<OperationCanceledException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockArgs.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldHandleGenericException()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetActivitySource(_activitySource);
        service.SetThrowGenericException(true);

        var mockReceiver = ServiceBusBackgroundServiceFaker.GenerateServiceBusReceiverMock();
        var mockArgs = ServiceBusBackgroundServiceFaker.GenerateProcessMessageEventArgsMock(mockReceiver.Object);

        // Act
        await service.TestHandleMessageAsync(mockArgs.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro inesperado ao processar mensagem")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockArgs.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleErrorAsync_ShouldLogError()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        var mockErrorArgs = ServiceBusBackgroundServiceFaker.GenerateProcessErrorEventArgsMock();

        // Act
        await service.TestHandleErrorAsync(mockErrorArgs.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro no processamento de mensagens do Service Bus")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);

        // Act & Assert
        service.Dispose(); // Should not throw any exception
    }

    [Fact]
    public void Dispose_WithInvalidOperationException_ShouldLogWarning()
    {
        // Arrange
        var service = new TestServiceBusBackgroundService(_loggerMock.Object, _configurationMock.Object, _requestConfiguration);
        service.SetThrowInvalidOperationExceptionOnDispose(true);

        // Act & Assert
        service.Dispose(); // Should not throw exception

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Erro ao fazer o dispose")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _activitySource?.Dispose();
    }
}

/// <summary>
/// Implementação de teste do ServiceBusBackgroundService para permitir testes unitários
/// </summary>
public class TestServiceBusBackgroundService : ServiceBusBackgroundService<TestServiceBusBackgroundService>
{
    private bool _executeRuleResult = true;
    private ServiceBusFailureReason? _throwServiceBusException;
    private bool _throwOperationCanceledException;
    private bool _throwGenericException;
    private bool _throwOnDispose;

    public TestServiceBusBackgroundService(
        ILogger<TestServiceBusBackgroundService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
    }

    protected override Task<bool> ExecuteRule(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        if (_throwServiceBusException.HasValue)
        {
            throw new ServiceBusException("Test ServiceBus exception", _throwServiceBusException.Value);
        }

        if (_throwOperationCanceledException)
        {
            throw new OperationCanceledException("Test operation was cancelled");
        }

        if (_throwGenericException)
        {
            throw new InvalidOperationException("Test generic exception");
        }

        return Task.FromResult(_executeRuleResult);
    }

    protected override void Dispose(bool disposing)
    {
        if (_throwOnDispose)
        {
            try
            {
                throw new InvalidOperationException("Test dispose exception");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Erro ao fazer o dispose dos recursos");
            }
        }

        base.Dispose(disposing);
    }

    // Métodos públicos para testes
    public RequestConfiguration GetRequestConfiguration() => RequestConfiguration;
    public ILogger<ServiceBusBackgroundService<TestServiceBusBackgroundService>> GetLogger() => Logger;
    public IConfigurationCustom GetConfigurationCustom() => ConfigurationCustom;

    public void SetActivitySource(ActivitySource activitySource)
    {
        ActivitySourceCustom = activitySource;
    }

    public void SetExecuteRuleResult(bool result)
    {
        _executeRuleResult = result;
    }

    public void SetThrowServiceBusException(ServiceBusFailureReason reason)
    {
        _throwServiceBusException = reason;
    }

    public void SetThrowOperationCanceledException(bool throwException)
    {
        _throwOperationCanceledException = throwException;
    }

    public void SetThrowGenericException(bool throwException)
    {
        _throwGenericException = throwException;
    }

    public void SetThrowInvalidOperationExceptionOnDispose(bool throwException)
    {
        _throwOnDispose = throwException;
    }

    public void TestConfigureServiceBus(string cnnName, string queueName, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(cnnName, queueName, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBus(string connectionString, string topicName, string subscription, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions, System.Func<string, string> getConnectionString = null)
    {
        // Para testar com topic e subscription usando connection string
        ConfigureServiceBus(connectionString, topicName, subscription, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBus(string topicName, string subscription, string fullyQualifiedNamespace, Azure.Core.TokenCredential credential, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(topicName, subscription, fullyQualifiedNamespace, credential, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBus(string queueName, string fullyQualifiedNamespace, Azure.Core.TokenCredential credential, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(queueName, fullyQualifiedNamespace, credential, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public Task TestHandleMessageAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
        => HandleMessageAsync(args, cancellationToken);

    public Task TestHandleErrorAsync(ProcessErrorEventArgs args)
        => HandleErrorAsync(args);
}
