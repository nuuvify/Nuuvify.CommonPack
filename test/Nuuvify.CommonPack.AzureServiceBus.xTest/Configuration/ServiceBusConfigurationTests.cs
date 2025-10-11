namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Configuration;

/// <summary>
/// Testes unitários para ServiceBusConfiguration
/// </summary>
[Trait("Category", "Unit")]
public class ServiceBusConfigurationTests : IClassFixture<ServiceBusTestFixture>
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly Faker _faker;

    public ServiceBusConfigurationTests(ServiceBusTestFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var config = new ServiceBusConfiguration();

        // Assert
        config.ConnectionString.ShouldBeEmpty();
        config.QueueName.ShouldBeEmpty();
        config.TopicName.ShouldBeEmpty();
        config.TopicSubscription.ShouldBeEmpty();
        config.OperationTimeoutSeconds.ShouldBe(30);
        config.MaxRetryAttempts.ShouldBe(3);
        config.RetryDelaySeconds.ShouldBe(5);
        config.MaxBatchSize.ShouldBe(100);
        config.EnableSessions.ShouldBeFalse();
        config.EnablePartitioning.ShouldBeFalse();
        config.DefaultMessageTtlMinutes.ShouldBe(60);
    }

    [Fact]
    public void ConnectionString_WhenSetToValidValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusConfiguration();
        var connectionString = _fixture.CreateValidConnectionString();

        // Act
        config.ConnectionString = connectionString;

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public void QueueName_WhenSetToValidValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusConfiguration();
        var queueName = _faker.Lorem.Word();

        // Act
        config.QueueName = queueName;

        // Assert
        config.QueueName.ShouldBe(queueName);
    }

    [Fact]
    public void TopicName_WhenSetToValidValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusConfiguration();
        var topicName = _faker.Lorem.Word();

        // Act
        config.TopicName = topicName;

        // Assert
        config.TopicName.ShouldBe(topicName);
    }

    [Fact]
    public void TopicSubscription_WhenSetToValidValue_ShouldReturnSameValue()
    {
        // Arrange
        var config = new ServiceBusConfiguration();
        var subscription = _faker.Lorem.Word();

        // Act
        config.TopicSubscription = subscription;

        // Assert
        config.TopicSubscription.ShouldBe(subscription);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(300)]
    public void OperationTimeoutSeconds_WhenSetToValidValue_ShouldReturnSameValue(int timeoutSeconds)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.OperationTimeoutSeconds = timeoutSeconds;

        // Assert
        config.OperationTimeoutSeconds.ShouldBe(timeoutSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetryAttempts_WhenSetToValidValue_ShouldReturnSameValue(int retryAttempts)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.MaxRetryAttempts = retryAttempts;

        // Assert
        config.MaxRetryAttempts.ShouldBe(retryAttempts);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(30)]
    public void RetryDelaySeconds_WhenSetToValidValue_ShouldReturnSameValue(int delaySeconds)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.RetryDelaySeconds = delaySeconds;

        // Assert
        config.RetryDelaySeconds.ShouldBe(delaySeconds);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    public void MaxBatchSize_WhenSetToValidValue_ShouldReturnSameValue(int batchSize)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.MaxBatchSize = batchSize;

        // Assert
        config.MaxBatchSize.ShouldBe(batchSize);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSessions_WhenSetToValue_ShouldReturnSameValue(bool enableSessions)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.EnableSessions = enableSessions;

        // Assert
        config.EnableSessions.ShouldBe(enableSessions);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnablePartitioning_WhenSetToValue_ShouldReturnSameValue(bool enablePartitioning)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.EnablePartitioning = enablePartitioning;

        // Assert
        config.EnablePartitioning.ShouldBe(enablePartitioning);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(1440)] // 24 hours
    public void DefaultMessageTtlMinutes_WhenSetToValidValue_ShouldReturnSameValue(int ttlMinutes)
    {
        // Arrange
        var config = new ServiceBusConfiguration();

        // Act
        config.DefaultMessageTtlMinutes = ttlMinutes;

        // Assert
        config.DefaultMessageTtlMinutes.ShouldBe(ttlMinutes);
    }

    [Fact]
    public void IsValid_WhenConfigurationIsComplete_ShouldReturnTrue()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            OperationTimeoutSeconds = 30,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 5,
            MaxBatchSize = 100,
            DefaultMessageTtlMinutes = 60
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenConnectionStringIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = "",
            OperationTimeoutSeconds = 30
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_WhenOperationTimeoutSecondsIsInvalid_ShouldReturnFalse(int timeout)
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            OperationTimeoutSeconds = timeout
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenMaxRetryAttemptsIsNegative_ShouldReturnFalse()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            MaxRetryAttempts = -1
        };

        // Act
        var result = config.IsValid();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateConfiguration_WhenConfigurationIsValid_ShouldReturnEmptyList()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = _fixture.CreateValidConnectionString(),
            OperationTimeoutSeconds = 30,
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 5,
            MaxBatchSize = 100,
            DefaultMessageTtlMinutes = 60
        };

        // Act
        var errors = config.ValidateConfiguration();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateConfiguration_WhenConnectionStringIsEmpty_ShouldReturnError()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = "",
            OperationTimeoutSeconds = 30
        };

        // Act
        var errors = config.ValidateConfiguration();

        // Assert
        errors.ShouldNotBeEmpty();
        errors.ShouldContain(error => error.Contains("ConnectionString é obrigatória"));
    }

    [Fact]
    public void ValidateConfiguration_WhenMultipleFieldsAreInvalid_ShouldReturnMultipleErrors()
    {
        // Arrange
        var config = new ServiceBusConfiguration
        {
            ConnectionString = "",
            OperationTimeoutSeconds = -1,
            MaxRetryAttempts = -1,
            RetryDelaySeconds = -1,
            MaxBatchSize = 0,
            DefaultMessageTtlMinutes = 0
        };

        // Act
        var errors = config.ValidateConfiguration();

        // Assert
        errors.Count.ShouldBe(6);
    }

    [Fact]
    public void ServiceBusConfiguration_ShouldAllowPropertyInitialization()
    {
        // Arrange
        var connectionString = _fixture.CreateValidConnectionString();
        var queueName = _faker.Lorem.Word();
        var topicName = _faker.Lorem.Word();
        var subscription = _faker.Lorem.Word();
        var timeout = _faker.Random.Int(1, 300);
        var maxRetries = _faker.Random.Int(0, 10);
        var retryDelay = _faker.Random.Int(0, 60);
        var batchSize = _faker.Random.Int(1, 500);
        var enableSessions = _faker.Random.Bool();
        var enablePartitioning = _faker.Random.Bool();
        var ttl = _faker.Random.Int(1, 1440);

        // Act
        var config = new ServiceBusConfiguration
        {
            ConnectionString = connectionString,
            QueueName = queueName,
            TopicName = topicName,
            TopicSubscription = subscription,
            OperationTimeoutSeconds = timeout,
            MaxRetryAttempts = maxRetries,
            RetryDelaySeconds = retryDelay,
            MaxBatchSize = batchSize,
            EnableSessions = enableSessions,
            EnablePartitioning = enablePartitioning,
            DefaultMessageTtlMinutes = ttl
        };

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
        config.QueueName.ShouldBe(queueName);
        config.TopicName.ShouldBe(topicName);
        config.TopicSubscription.ShouldBe(subscription);
        config.OperationTimeoutSeconds.ShouldBe(timeout);
        config.MaxRetryAttempts.ShouldBe(maxRetries);
        config.RetryDelaySeconds.ShouldBe(retryDelay);
        config.MaxBatchSize.ShouldBe(batchSize);
        config.EnableSessions.ShouldBe(enableSessions);
        config.EnablePartitioning.ShouldBe(enablePartitioning);
        config.DefaultMessageTtlMinutes.ShouldBe(ttl);
    }
}
