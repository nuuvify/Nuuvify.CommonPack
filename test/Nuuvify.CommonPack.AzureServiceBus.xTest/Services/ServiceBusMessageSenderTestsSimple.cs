namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

/// <summary>
/// Testes unitários para ServiceBusMessageSender (foco em validações de parâmetros)
/// </summary>
[Trait("Category", "Unit")]
public class ServiceBusMessageSenderTestsSimple : IClassFixture<ServiceBusTestFixture>
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Faker _faker;

    public ServiceBusMessageSenderTestsSimple(ServiceBusTestFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => new ServiceBusMessageSender(null, _fixture.CreateMockLogger<ServiceBusMessageSender>().Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = Options.Create(new ServiceBusConfiguration
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=test="
        });

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => new ServiceBusMessageSender(config, null));
    }

    [Fact]
    public void Constructor_WithInvalidConnectionString_ShouldThrowFormatException()
    {
        // Arrange
        var config = Options.Create(new ServiceBusConfiguration
        {
            ConnectionString = "invalid-connection-string"
        });

        // Act & Assert - Azure SDK throws FormatException for malformed connection strings
        _ = Should.Throw<FormatException>(() => new ServiceBusMessageSender(config, _fixture.CreateMockLogger<ServiceBusMessageSender>().Object));
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ShouldThrowArgumentException()
    {
        // Arrange
        var config = Options.Create(new ServiceBusConfiguration
        {
            ConnectionString = ""
        });

        // Act & Assert
        _ = Should.Throw<ArgumentException>(() => new ServiceBusMessageSender(config, _fixture.CreateMockLogger<ServiceBusMessageSender>().Object));
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void ServiceBusConfiguration_IsValid_WithValidConfiguration_ShouldReturnTrue()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void ServiceBusConfiguration_IsValid_WithInvalidConfiguration_ShouldReturnFalse()
    {
        // Arrange
        var config = _fixture.CreateInvalidConfiguration();

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ServiceBusConfiguration_WithNullConnectionString_ShouldBeInvalid()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = null,
            QueueName = "test-queue"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ServiceBusConfiguration_WithEmptyConnectionString_ShouldBeInvalid()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = "",
            QueueName = "test-queue"
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ServiceBusConfiguration_WithInvalidTimeout_ShouldBeInvalid()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            QueueName = "test-queue",
            OperationTimeoutSeconds = -1
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ServiceBusConfiguration_WithInvalidRetryAttempts_ShouldBeInvalid()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            QueueName = "test-queue",
            MaxRetryAttempts = -1
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Helper Object Tests

    [Fact]
    public void ServiceBusMessageOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ServiceBusMessageOptions();

        // Assert
        options.MessageId.ShouldBeNull();
        options.CorrelationId.ShouldBeNull();
        options.ContentType.ShouldBe("application/json"); // Default value set in the class
        options.Subject.ShouldBeNull();
        options.TimeToLive.ShouldBe(default);
        _ = options.ApplicationProperties.ShouldNotBeNull();
        options.ApplicationProperties.Count.ShouldBe(0);
    }

    [Fact]
    public void ServiceBusMessageOptions_WithCustomValues_ShouldRetainValues()
    {
        // Arrange
        var messageId = _faker.Random.Guid().ToString();
        var correlationId = _faker.Random.Guid().ToString();
        var contentType = "application/json";
        var subject = _faker.Lorem.Word();
        var ttl = TimeSpan.FromMinutes(30);
        var customProp = _faker.Lorem.Word();

        // Act
        var options = new ServiceBusMessageOptions
        {
            MessageId = messageId,
            CorrelationId = correlationId,
            ContentType = contentType,
            Subject = subject,
            TimeToLive = ttl,
            ApplicationProperties = new Dictionary<string, object> { { "test", customProp } }
        };

        // Assert
        options.MessageId.ShouldBe(messageId);
        options.CorrelationId.ShouldBe(correlationId);
        options.ContentType.ShouldBe(contentType);
        options.Subject.ShouldBe(subject);
        options.TimeToLive.ShouldBe(ttl);
        options.ApplicationProperties["test"].ShouldBe(customProp);
    }

    [Fact]
    public void ServiceBusOperationOptions_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new ServiceBusOperationOptions();

        // Assert
        options.UseTemporaryClient.ShouldBeTrue(); // Default value is true in the class
    }

    [Fact]
    public void ServiceBusOperationOptions_WithCustomValues_ShouldRetainValues()
    {
        // Arrange & Act
        var options = new ServiceBusOperationOptions
        {
            UseTemporaryClient = true
        };

        // Assert
        options.UseTemporaryClient.ShouldBeTrue();
    }

    #endregion

    #region Fixture Helper Tests

    [Fact]
    public void CreateTestMessage_ShouldReturnValidObject()
    {
        // Arrange & Act
        var message = _fixture.CreateTestMessage();

        // Assert
        _ = message.ShouldNotBeNull();
    }

    [Fact]
    public void CreateTestMessages_ShouldReturnValidCollection()
    {
        // Arrange
        var count = 3;

        // Act
        var messages = _fixture.CreateTestMessages(count);

        // Assert
        _ = messages.ShouldNotBeNull();
        messages.Count.ShouldBe(count);
        messages.All(m => m != null).ShouldBeTrue();
    }

    [Fact]
    public void CreateValidConnectionString_ShouldReturnValidFormat()
    {
        // Arrange & Act
        var connectionString = _fixture.CreateValidConnectionString();

        // Assert
        _ = connectionString.ShouldNotBeNull();
        connectionString.ShouldContain("Endpoint=");
        connectionString.ShouldContain("SharedAccessKeyName=");
        connectionString.ShouldContain("SharedAccessKey=");
    }

    #endregion

    #region ServiceBusClientConfiguration Tests

    [Fact]
    public void ServiceBusClientConfiguration_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new ServiceBusClientConfiguration();

        // Assert
        config.ConnectionString.ShouldBe(string.Empty); // Default value is empty string
        config.QueueName.ShouldBe(string.Empty); // Default value is empty string
        config.TopicName.ShouldBe(string.Empty); // Default value is empty string
        config.TransportType.ShouldBe(Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets); // Default value
        config.ReuseConnections.ShouldBeTrue(); // Default value is true
    }

    [Fact]
    public void ServiceBusClientConfiguration_WithCustomValues_ShouldRetainValues()
    {
        // Arrange
        var connectionString = _fixture.CreateValidConnectionString();
        var queueName = _faker.Lorem.Word();
        var topicName = _faker.Lorem.Word();

        // Act
        var config = new ServiceBusClientConfiguration
        {
            ConnectionString = connectionString,
            QueueName = queueName,
            TopicName = topicName,
            TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets,
            ReuseConnections = true
        };

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
        config.QueueName.ShouldBe(queueName);
        config.TopicName.ShouldBe(topicName);
        config.TransportType.ShouldBe(Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);
        config.ReuseConnections.ShouldBeTrue();
    }

    #endregion

    #region Data Generation Tests

    [Fact]
    public void Faker_GeneratesValidQueueNames()
    {
        // Act
        var queueName1 = _fixture.CreateQueueName();
        var queueName2 = _fixture.CreateQueueName();

        // Assert
        queueName1.ShouldNotBeNullOrWhiteSpace();
        queueName2.ShouldNotBeNullOrWhiteSpace();
        queueName1.ShouldNotBe(queueName2); // Should generate different names
        queueName1.Length.ShouldBeGreaterThan(0);
        queueName2.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Faker_GeneratesValidTopicNames()
    {
        // Act
        var topicName1 = _fixture.CreateTopicName();
        var topicName2 = _fixture.CreateTopicName();

        // Assert
        topicName1.ShouldNotBeNullOrWhiteSpace();
        topicName2.ShouldNotBeNullOrWhiteSpace();
        topicName1.ShouldNotBe(topicName2); // Should generate different names
    }

    [Fact]
    public void ServiceBusMessageOptions_CreatedByFixture_ShouldBeValid()
    {
        // Act
        var options = _fixture.CreateServiceBusMessageOptions();

        // Assert
        _ = options.ShouldNotBeNull();
        options.MessageId.ShouldNotBeNullOrWhiteSpace();
        options.CorrelationId.ShouldNotBeNullOrWhiteSpace();
        options.ContentType.ShouldBe("application/json");
        options.Subject.ShouldNotBeNullOrWhiteSpace();
        options.TimeToLive.ShouldBeGreaterThan(TimeSpan.Zero);
        _ = options.ApplicationProperties.ShouldNotBeNull();
        options.ApplicationProperties.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ServiceBusOperationOptions_CreatedByFixture_ShouldBeValid()
    {
        // Act
        var options = _fixture.CreateServiceBusOperationOptions();

        // Assert
        _ = options.ShouldNotBeNull();
        // UseTemporaryClient can be true or false, both are valid
    }

    #endregion

    #region Mock Tests

    [Fact]
    public void CreateMockLogger_ShouldReturnValidMock()
    {
        // Act
        var mockLogger = _fixture.CreateMockLogger<ServiceBusMessageSender>();

        // Assert
        _ = mockLogger.ShouldNotBeNull();
        _ = mockLogger.Object.ShouldNotBeNull();
    }

    [Fact]
    public void CreateConfigurationOptionsMock_ShouldReturnValidMock()
    {
        // Arrange
        var config = _fixture.CreateValidConfiguration();

        // Act
        var mock = _fixture.CreateConfigurationOptionsMock(config);

        // Assert
        _ = mock.ShouldNotBeNull();
        _ = mock.Object.ShouldNotBeNull();
        mock.Object.Value.ShouldBe(config);
    }

    [Fact]
    public void CreateClientConfigurationOptionsMock_ShouldReturnValidMock()
    {
        // Arrange
        var config = _fixture.CreateValidClientConfiguration();

        // Act
        var mock = _fixture.CreateClientConfigurationOptionsMock(config);

        // Assert
        _ = mock.ShouldNotBeNull();
        _ = mock.Object.ShouldNotBeNull();
        mock.Object.Value.ShouldBe(config);
    }

    #endregion
}
