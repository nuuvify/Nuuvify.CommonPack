namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Configuration;

/// <summary>
/// Testes unitários para ServiceBusClientConfiguration
/// </summary>
[Trait("Category", "Unit")]
public class ServiceBusClientConfigurationTests : IClassFixture<ServiceBusTestFixture>
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Faker _faker;

    public ServiceBusClientConfigurationTests(ServiceBusTestFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var config = new ServiceBusClientConfiguration();

        // Assert
        config.ClientOptions.ShouldBeNull();
        config.PreConfiguredClient.ShouldBeNull();
        config.ClientFactory.ShouldBeNull();
        config.ReuseConnections.ShouldBeTrue();
        config.TransportType.ShouldBe(ServiceBusTransportType.AmqpWebSockets);
        config.RetryOptions.ShouldBeNull();
        config.WebProxy.ShouldBeNull();
    }

    [Fact]
    public void ClientOptions_WhenSetToValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var clientOptions = new ServiceBusClientOptions();

        // Act
        config.ClientOptions = clientOptions;

        // Assert
        config.ClientOptions.ShouldBeSameAs(clientOptions);
    }

    [Fact]
    public void PreConfiguredClient_WhenSetToValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var mockClient = _fixture.CreateServiceBusClient();

        // Act
        config.PreConfiguredClient = mockClient;

        // Assert
        config.PreConfiguredClient.ShouldBeSameAs(mockClient);
    }

    [Fact]
    public void ClientFactory_WhenSetToValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        Func<string, ServiceBusClientOptions, ServiceBusClient?> factory = (connectionString, options) =>
            _fixture.CreateServiceBusClient();

        // Act
        config.ClientFactory = factory;

        // Assert
        config.ClientFactory.ShouldBeSameAs(factory);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReuseConnections_WhenSetToValue_ShouldReturnSameValue(bool reuseConnections)
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();

        // Act
        config.ReuseConnections = reuseConnections;

        // Assert
        config.ReuseConnections.ShouldBe(reuseConnections);
    }

    [Theory]
    [InlineData(ServiceBusTransportType.AmqpTcp)]
    [InlineData(ServiceBusTransportType.AmqpWebSockets)]
    public void TransportType_WhenSetToValue_ShouldReturnSameValue(ServiceBusTransportType transportType)
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();

        // Act
        config.TransportType = transportType;

        // Assert
        config.TransportType.ShouldBe(transportType);
    }

    [Fact]
    public void RetryOptions_WhenSetToValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var retryOptions = new ServiceBusRetryOptions();

        // Act
        config.RetryOptions = retryOptions;

        // Assert
        config.RetryOptions.ShouldBeSameAs(retryOptions);
    }

    [Fact]
    public void WebProxy_WhenSetToValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var webProxy = new System.Net.WebProxy("http://proxy.example.com:8080");

        // Act
        config.WebProxy = webProxy;

        // Assert
        config.WebProxy.ShouldBeSameAs(webProxy);
    }

    [Fact]
    public void CreateClientOptions_WhenClientOptionsIsNull_ShouldCreateNewOptions()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration
        {
            TransportType = ServiceBusTransportType.AmqpTcp,
            RetryOptions = new ServiceBusRetryOptions(),
            WebProxy = new System.Net.WebProxy("http://proxy.example.com:8080")
        };

        // Act
        var result = config.CreateClientOptions();

        // Assert
        result.ShouldNotBeNull();
        result.TransportType.ShouldBe(ServiceBusTransportType.AmqpTcp);
        result.RetryOptions.ShouldNotBeNull();
        result.WebProxy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateClientOptions_WhenClientOptionsExists_ShouldUseExistingOptions()
    {
        // Arrange
        var existingOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        var config = new ServiceBusClientConfiguration
        {
            ClientOptions = existingOptions,
            TransportType = ServiceBusTransportType.AmqpTcp // Should not override existing
        };

        // Act
        var result = config.CreateClientOptions();

        // Assert
        result.ShouldBeSameAs(existingOptions);
        result.TransportType.ShouldBe(ServiceBusTransportType.AmqpWebSockets);
    }

    [Fact]
    public void CreateClientOptions_WhenExistingOptionsHaveDefaultValues_ShouldApplyConfiguration()
    {
        // Arrange
        var existingOptions = new ServiceBusClientOptions(); // Default transport type
        var retryOptions = new ServiceBusRetryOptions();
        var webProxy = new System.Net.WebProxy("http://proxy.example.com:8080");

        var config = new ServiceBusClientConfiguration
        {
            ClientOptions = existingOptions,
            TransportType = ServiceBusTransportType.AmqpTcp,
            RetryOptions = retryOptions,
            WebProxy = webProxy
        };

        // Act
        var result = config.CreateClientOptions();

        // Assert
        result.ShouldBeSameAs(existingOptions);
        result.TransportType.ShouldBe(ServiceBusTransportType.AmqpTcp);
        result.RetryOptions.ShouldNotBeNull();
        result.WebProxy.ShouldNotBeNull();
    }

    [Fact]
    public void CreateClient_WhenPreConfiguredClientExists_ShouldReturnPreConfiguredClient()
    {
        // Arrange
        var mockClient = new Mock<ServiceBusClient>();
        var preConfiguredClient = mockClient.Object;
        var config = new ServiceBusClientConfiguration
        {
            PreConfiguredClient = preConfiguredClient,
            ConnectionString = _fixture.CreateValidConnectionString(),
            ClientFactory = (cs, opts) => throw new InvalidOperationException("Should not be called")
        };

        // Act
        var result = config.CreateClient();

        // Assert
        result.ShouldBeSameAs(preConfiguredClient);
    }

    [Fact]
    public void CreateClient_WhenClientFactoryExists_ShouldUseFactory()
    {
        // Arrange
        var expectedClient = _fixture.CreateServiceBusClient();
        var connectionString = _fixture.CreateValidConnectionString();
        var capturedConnectionString = string.Empty;
        ServiceBusClientOptions? capturedOptions = null;

        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = connectionString,
            TransportType = ServiceBusTransportType.AmqpTcp,
            ClientFactory = (cs, opts) =>
            {
                capturedConnectionString = cs;
                capturedOptions = opts;
                return expectedClient;
            }
        };

        // Act
        var result = config.CreateClient();

        // Assert
        result.ShouldBeSameAs(expectedClient);
        capturedConnectionString.ShouldBe(connectionString);
        capturedOptions.ShouldNotBeNull();
        capturedOptions!.TransportType.ShouldBe(ServiceBusTransportType.AmqpTcp);
    }

    [Fact]
    public void CreateClient_WhenNoFactoryOrPreConfigured_ShouldCreateDefaultClient()
    {
        // Arrange
        var connectionString = _fixture.CreateValidConnectionString();
        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = connectionString,
            TransportType = ServiceBusTransportType.AmqpTcp
        };

        // Act
        var result = config.CreateClient();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ServiceBusClient>();
    }

    [Fact]
    public void IsValid_WhenHasPreConfiguredClient_ShouldReturnTrue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration
        {
            PreConfiguredClient = _fixture.CreateServiceBusClient(),
            ConnectionString = _fixture.CreateValidConnectionString() // Need valid base config
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenHasClientFactoryAndConnectionString_ShouldReturnTrue()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            ClientFactory = (cs, opts) => _fixture.CreateServiceBusClient()
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenHasClientFactoryButNoConnectionString_ShouldReturnFalse()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = "",
            ClientFactory = (cs, opts) => _fixture.CreateServiceBusClient()
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenBaseConfigurationIsInvalid_ShouldReturnFalse()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            OperationTimeoutSeconds = -1 // Invalid value
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void InheritsFromServiceBusConfiguration_ShouldHaveAllBaseProperties()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var connectionString = _fixture.CreateValidConnectionString();
        var queueName = _faker.Lorem.Word();

        // Act
        config.ConnectionString = connectionString;
        config.QueueName = queueName;
        config.OperationTimeoutSeconds = 45;

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
        config.QueueName.ShouldBe(queueName);
        config.OperationTimeoutSeconds.ShouldBe(45);
    }

    [Fact]
    public void ServiceBusClientConfiguration_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var config = new ServiceBusClientConfiguration();
        var clientOptions = new ServiceBusClientOptions();
        var preConfiguredClient = _fixture.CreateServiceBusClient();
        Func<string, ServiceBusClientOptions, ServiceBusClient> clientFactory = (cs, opts) => _fixture.CreateServiceBusClient();
        var retryOptions = new ServiceBusRetryOptions();
        var webProxy = new System.Net.WebProxy("http://proxy.example.com:8080");

        // Act & Assert
        config.ClientOptions = clientOptions;
        config.ClientOptions.ShouldBeSameAs(clientOptions);

        config.PreConfiguredClient = preConfiguredClient;
        config.PreConfiguredClient.ShouldBeSameAs(preConfiguredClient);

        config.ClientFactory = clientFactory;
        config.ClientFactory.ShouldBeSameAs(clientFactory);

        config.ReuseConnections = false;
        config.ReuseConnections.ShouldBeFalse();

        config.TransportType = ServiceBusTransportType.AmqpTcp;
        config.TransportType.ShouldBe(ServiceBusTransportType.AmqpTcp);

        config.RetryOptions = retryOptions;
        config.RetryOptions.ShouldBeSameAs(retryOptions);

        config.WebProxy = webProxy;
        config.WebProxy.ShouldBeSameAs(webProxy);
    }
}
