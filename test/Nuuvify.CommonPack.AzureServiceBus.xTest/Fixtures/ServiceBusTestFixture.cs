namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Fixtures;

/// <summary>
/// Fixture base para testes do Azure Service Bus com dados fake e configurações comuns
/// </summary>
public class ServiceBusTestFixture
{
    private readonly Faker _faker;

    public ServiceBusTestFixture()
    {
        _faker = new Faker("pt_BR");
    }

    /// <summary>
    /// Cria uma configuração válida do Service Bus para testes
    /// </summary>
    public ServiceBusConfiguration CreateValidConfiguration()
    {
        return new ServiceBusConfiguration
        {
            ConnectionString = CreateValidConnectionString(),
            QueueName = _faker.Random.AlphaNumeric(10).ToLowerInvariant(),
            TopicName = _faker.Random.AlphaNumeric(10).ToLowerInvariant(),
            TopicSubscription = _faker.Random.AlphaNumeric(10).ToLowerInvariant(),
            OperationTimeoutSeconds = _faker.Random.Int(10, 120),
            MaxRetryAttempts = _faker.Random.Int(1, 10),
            RetryDelaySeconds = _faker.Random.Int(1, 30),
            MaxBatchSize = _faker.Random.Int(10, 1000),
            DefaultMessageTtlMinutes = _faker.Random.Int(10, 1440),
            EnableSessions = _faker.Random.Bool(),
            EnablePartitioning = _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Cria uma configuração inválida do Service Bus para testes
    /// </summary>
    public ServiceBusConfiguration CreateInvalidConfiguration()
    {
        return new ServiceBusConfiguration
        {
            ConnectionString = "", // Inválida - string vazia
            QueueName = _faker.Random.AlphaNumeric(10),
            TopicName = _faker.Random.AlphaNumeric(10),
            OperationTimeoutSeconds = -1, // Inválido - valor negativo
            MaxRetryAttempts = -1, // Inválido - valor negativo
            RetryDelaySeconds = -1, // Inválido - valor negativo
            MaxBatchSize = 0, // Inválido - zero
            DefaultMessageTtlMinutes = -1 // Inválido - valor negativo
        };
    }

    /// <summary>
    /// Cria uma configuração de cliente válida para testes
    /// </summary>
    public ServiceBusClientConfiguration CreateValidClientConfiguration()
    {
        return new ServiceBusClientConfiguration
        {
            ConnectionString = CreateValidConnectionString(),
            QueueName = _faker.Random.AlphaNumeric(10).ToLowerInvariant(),
            TopicName = _faker.Random.AlphaNumeric(10).ToLowerInvariant(),
            OperationTimeoutSeconds = _faker.Random.Int(10, 120),
            MaxRetryAttempts = _faker.Random.Int(1, 10),
            RetryDelaySeconds = _faker.Random.Int(1, 30),
            MaxBatchSize = _faker.Random.Int(10, 1000),
            DefaultMessageTtlMinutes = _faker.Random.Int(10, 1440),
            TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets,
            ReuseConnections = _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Cria uma connection string válida para testes
    /// </summary>
    public string CreateValidConnectionString()
    {
        var endpoint = $"sb://{_faker.Internet.DomainName()}.servicebus.windows.net";
        var keyName = "RootManageSharedAccessKey";
        var key = _faker.Random.AlphaNumeric(44);

        return $"Endpoint={endpoint}/;SharedAccessKeyName={keyName};SharedAccessKey={key}";
    }

    /// <summary>
    /// Cria um mock do IOptions<ServiceBusConfiguration>
    /// </summary>
    public Mock<IOptions<ServiceBusConfiguration>> CreateConfigurationOptionsMock(ServiceBusConfiguration config)
    {
        var mock = new Mock<IOptions<ServiceBusConfiguration>>();
        _ = mock.Setup(x => x.Value).Returns(config);
        return mock;
    }

    /// <summary>
    /// Cria um mock do IOptions<ServiceBusClientConfiguration>
    /// </summary>
    public Mock<IOptions<ServiceBusClientConfiguration>> CreateClientConfigurationOptionsMock(ServiceBusClientConfiguration config)
    {
        var mock = new Mock<IOptions<ServiceBusClientConfiguration>>();
        _ = mock.Setup(x => x.Value).Returns(config);
        return mock;
    }

    /// <summary>
    /// Cria um mock de ILogger para testes
    /// </summary>
    public Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Cria um ServiceBusClient usando uma connection string válida para testes
    /// </summary>
    public ServiceBusClient? CreateServiceBusClient()
    {
        // Para testes unitários, retornamos null pois não queremos conexão real
        // Em testes de integração, poderia usar uma connection string de teste
        return null;
    }

    /// <summary>
    /// Cria dados de mensagem fake para testes
    /// </summary>
    public object CreateTestMessage()
    {
        return new
        {
            Id = _faker.Random.Int(1, 1000),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Message = _faker.Lorem.Sentence(),
            Timestamp = _faker.Date.Recent(),
            IsActive = _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Cria uma lista de mensagens fake para testes de lote
    /// </summary>
    public IReadOnlyCollection<object> CreateTestMessages(int count = 5)
    {
        var messages = new List<object>();
        for (int i = 0; i < count; i++)
        {
            messages.Add(CreateTestMessage());
        }
        return messages;
    }

    /// <summary>
    /// Cria nomes de filas aleatórios para testes
    /// </summary>
    public string CreateQueueName() => _faker.Random.AlphaNumeric(10).ToLowerInvariant();

    /// <summary>
    /// Cria nomes de tópicos aleatórios para testes
    /// </summary>
    public string CreateTopicName() => _faker.Random.AlphaNumeric(10).ToLowerInvariant();

    /// <summary>
    /// Cria um IOptions<ServiceBusConfiguration> válido para testes
    /// </summary>
    public IOptions<ServiceBusConfiguration> CreateValidServiceBusConfiguration()
    {
        var config = CreateValidConfiguration();
        var mock = CreateConfigurationOptionsMock(config);
        return mock.Object;
    }

    /// <summary>
    /// Cria um IOptions<ServiceBusConfiguration> inválido para testes
    /// </summary>
    public IOptions<ServiceBusConfiguration> CreateInvalidServiceBusConfiguration()
    {
        var config = CreateInvalidConfiguration();
        var mock = CreateConfigurationOptionsMock(config);
        return mock.Object;
    }

    /// <summary>
    /// Cria um IOptions<ServiceBusClientConfiguration> válido para testes
    /// </summary>
    public IOptions<ServiceBusClientConfiguration> CreateValidServiceBusClientConfiguration()
    {
        var config = CreateValidClientConfiguration();
        var mock = CreateClientConfigurationOptionsMock(config);
        return mock.Object;
    }

    /// <summary>
    /// Cria uma mensagem de exemplo para testes
    /// </summary>
    public object CreateSampleMessage()
    {
        return CreateTestMessage();
    }

    /// <summary>
    /// Cria uma lista de mensagens de exemplo para testes
    /// </summary>
    public IReadOnlyCollection<object> CreateSampleMessageList(int count = 5)
    {
        return CreateTestMessages(count);
    }

    /// <summary>
    /// Cria opções de mensagem do Service Bus para testes
    /// </summary>
    public ServiceBusMessageOptions CreateServiceBusMessageOptions()
    {
        return new ServiceBusMessageOptions
        {
            MessageId = _faker.Random.Guid().ToString(),
            CorrelationId = _faker.Random.Guid().ToString(),
            ContentType = "application/json",
            Subject = _faker.Lorem.Word(),
            TimeToLive = TimeSpan.FromMinutes(_faker.Random.Int(1, 60)),
            ApplicationProperties = new Dictionary<string, object>
            {
                { "customProperty1", _faker.Lorem.Word() },
                { "customProperty2", _faker.Random.Int(1, 100) }
            }
        };
    }

    /// <summary>
    /// Cria opções de operação do Service Bus para testes
    /// </summary>
    public ServiceBusOperationOptions CreateServiceBusOperationOptions()
    {
        return new ServiceBusOperationOptions
        {
            UseTemporaryClient = _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Cria uma instância do ServiceBusMessageSender para testes
    /// </summary>
    public ServiceBusMessageSender CreateServiceBusMessageSender()
    {
        var config = CreateValidServiceBusConfiguration();
        var logger = CreateMockLogger<ServiceBusMessageSender>();

        return new ServiceBusMessageSender(config, logger.Object);
    }
}
