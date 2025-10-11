namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

/// <summary>
/// Testes unitários para ServiceBusConfigurationManager
/// </summary>
[Trait("Category", "Unit")]
public class ServiceBusConfigurationManagerTests : IClassFixture<ServiceBusTestFixture>
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Faker _faker;

    public ServiceBusConfigurationManagerTests(ServiceBusTestFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBaseConfiguration_ShouldCreateManager()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Assert
        _ = manager.ShouldNotBeNull();
        _ = manager.BaseConfiguration.ShouldNotBeNull();
        manager.HasClientConfiguration.ShouldBeFalse();
        manager.ClientConfiguration.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithValidConfigurations_ShouldCreateManagerWithClientConfig()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Assert
        _ = manager.ShouldNotBeNull();
        _ = manager.BaseConfiguration.ShouldNotBeNull();
        manager.HasClientConfiguration.ShouldBeTrue();
        _ = manager.ClientConfiguration.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBaseOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new ServiceBusConfigurationManager(null, null, logger.Object));
        exception.ParamName.ShouldBe("baseOptions");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new ServiceBusConfigurationManager(baseConfig, null, null));
        exception.ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithInvalidBaseConfiguration_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidConfig = _fixture.CreateInvalidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() =>
            new ServiceBusConfigurationManager(invalidConfig, null, logger.Object));
    }

    #endregion

    #region CreateServiceBusClient Tests

    [Fact]
    public async Task CreateServiceBusClient_WithBaseConfigurationOnly_ShouldCreateClient()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var client = manager.CreateServiceBusClient();

        // Assert
        _ = client.ShouldNotBeNull();
        await client.DisposeAsync(); // Clean up
    }

    [Fact]
    public async Task CreateServiceBusClient_WithClientConfiguration_ShouldCreateClientWithAdvancedConfig()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act
        var client = manager.CreateServiceBusClient();

        // Assert
        _ = client.ShouldNotBeNull();
        await client.DisposeAsync(); // Clean up
    }

    [Fact]
    public async Task CreateServiceBusClient_WithPreConfiguredClient_ShouldReturnPreConfiguredClient()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var preConfiguredClient = new ServiceBusClient(_fixture.CreateValidConnectionString());
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        clientConfig.Value.PreConfiguredClient = preConfiguredClient;
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act
        var client = manager.CreateServiceBusClient();

        // Assert
        client.ShouldBe(preConfiguredClient);
        await client.DisposeAsync(); // Clean up
    }

    #endregion

    #region Configuration Info Tests

    [Fact]
    public void GetTransportType_WithBaseConfigurationOnly_ShouldReturnDefault()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var transportType = manager.GetTransportType();

        // Assert
        transportType.ShouldBe(Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);
    }

    [Fact]
    public void GetTransportType_WithClientConfiguration_ShouldReturnClientTransportType()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        clientConfig.Value.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpTcp;
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act
        var transportType = manager.GetTransportType();

        // Assert
        transportType.ShouldBe(Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpTcp);
    }

    [Fact]
    public void GetConfigurationInfo_WithBaseConfigurationOnly_ShouldReturnBaseInfo()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var (timeout, maxRetry, transport) = manager.GetConfigurationInfo();

        // Assert
        timeout.ShouldBe(baseConfig.Value.OperationTimeoutSeconds);
        maxRetry.ShouldBe(baseConfig.Value.MaxRetryAttempts);
        transport.ShouldBe(Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);
    }

    [Fact]
    public void GetConfigurationInfo_WithClientConfiguration_ShouldReturnClientInfo()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act
        var (timeout, maxRetry, transport) = manager.GetConfigurationInfo();

        // Assert
        timeout.ShouldBe(clientConfig.Value.OperationTimeoutSeconds);
        maxRetry.ShouldBe(clientConfig.Value.MaxRetryAttempts);
        transport.ShouldBe(clientConfig.Value.TransportType);
    }

    #endregion

    #region Configuration Fallback Tests

    [Fact]
    public void Constructor_WithEmptyClientConfiguration_ShouldApplyFallbacks()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var emptyClientConfig = Options.Create(new ServiceBusClientConfiguration
        {
            // Empty values that should get fallbacks from base config
            ConnectionString = "",
            QueueName = "",
            TopicName = "",
            OperationTimeoutSeconds = 0,
            MaxRetryAttempts = 0
        });
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act
        var manager = new ServiceBusConfigurationManager(baseConfig, emptyClientConfig, logger.Object);

        // Assert
        manager.ClientConfiguration.ConnectionString.ShouldBe(baseConfig.Value.ConnectionString);
        manager.ClientConfiguration.QueueName.ShouldBe(baseConfig.Value.QueueName);
        manager.ClientConfiguration.TopicName.ShouldBe(baseConfig.Value.TopicName);
        manager.ClientConfiguration.OperationTimeoutSeconds.ShouldBe(baseConfig.Value.OperationTimeoutSeconds);
        manager.ClientConfiguration.MaxRetryAttempts.ShouldBe(baseConfig.Value.MaxRetryAttempts);
    }

    [Fact]
    public void Constructor_WithPartialClientConfiguration_ShouldApplySelectiveFallbacks()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var customConnectionString = _fixture.CreateValidConnectionString();
        var customTimeout = _faker.Random.Int(60, 120);

        var partialClientConfig = Options.Create(new ServiceBusClientConfiguration
        {
            ConnectionString = customConnectionString, // Custom value
            QueueName = "", // Should fallback to base
            OperationTimeoutSeconds = customTimeout, // Custom value
            MaxRetryAttempts = 0 // Should fallback to base
        });
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act
        var manager = new ServiceBusConfigurationManager(baseConfig, partialClientConfig, logger.Object);

        // Assert
        manager.ClientConfiguration.ConnectionString.ShouldBe(customConnectionString);
        manager.ClientConfiguration.QueueName.ShouldBe(baseConfig.Value.QueueName); // Fallback
        manager.ClientConfiguration.OperationTimeoutSeconds.ShouldBe(customTimeout);
        manager.ClientConfiguration.MaxRetryAttempts.ShouldBe(baseConfig.Value.MaxRetryAttempts); // Fallback
    }

    #endregion

    #region Static Validation Tests

    [Fact]
    public void ValidateConfiguration_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var validConfig = _fixture.CreateValidConfiguration();

        // Act & Assert
        Should.NotThrow(() => ServiceBusConfigurationManager.ValidateConfiguration(validConfig));
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidConfig = _fixture.CreateInvalidConfiguration();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            ServiceBusConfigurationManager.ValidateConfiguration(invalidConfig));

        exception.ParamName.ShouldBe("configuration");
        exception.Message.ShouldContain("Configuração inválida");
    }

    [Fact]
    public void ValidateConfiguration_WithNullConnectionString_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = null
        };

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() =>
            ServiceBusConfigurationManager.ValidateConfiguration(config));
    }

    [Fact]
    public void ValidateConfiguration_WithEmptyConnectionString_ShouldThrowArgumentException()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = ""
        };

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() =>
            ServiceBusConfigurationManager.ValidateConfiguration(config));
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void HasClientConfiguration_WithoutClientConfig_ShouldReturnFalse()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act & Assert
        manager.HasClientConfiguration.ShouldBeFalse();
    }

    [Fact]
    public void HasClientConfiguration_WithClientConfig_ShouldReturnTrue()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act & Assert
        manager.HasClientConfiguration.ShouldBeTrue();
    }

    [Fact]
    public void BaseConfiguration_ShouldReturnProcessedConfiguration()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var result = manager.BaseConfiguration;

        // Assert
        _ = result.ShouldNotBeNull();
        result.ConnectionString.ShouldBe(baseConfig.Value.ConnectionString);
    }

    [Fact]
    public void ClientConfiguration_WithoutClientConfig_ShouldReturnNull()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var result = manager.ClientConfiguration;

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ClientConfiguration_WithClientConfig_ShouldReturnProcessedConfiguration()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var clientConfig = _fixture.CreateValidServiceBusClientConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, clientConfig, logger.Object);

        // Act
        var result = manager.ClientConfiguration;

        // Assert
        _ = result.ShouldNotBeNull();
        result.ConnectionString.ShouldBe(clientConfig.Value.ConnectionString);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Constructor_WithNegativeTimeoutInBase_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidConfig = Options.Create(new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            OperationTimeoutSeconds = -1
        });
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() =>
            new ServiceBusConfigurationManager(invalidConfig, null, logger.Object));
    }

    [Fact]
    public void Constructor_WithNegativeRetryAttemptsInBase_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidConfig = Options.Create(new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            MaxRetryAttempts = -1
        });
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() =>
            new ServiceBusConfigurationManager(invalidConfig, null, logger.Object));
    }

    [Fact]
    public async Task CreateServiceBusClient_MultipleInvocations_ShouldCreateDifferentInstances()
    {
        // Arrange
        var baseConfig = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusConfigurationManager>();
        var manager = new ServiceBusConfigurationManager(baseConfig, null, logger.Object);

        // Act
        var client1 = manager.CreateServiceBusClient();
        var client2 = manager.CreateServiceBusClient();

        // Assert
        client1.ShouldNotBeSameAs(client2);

        // Clean up
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    #endregion
}
