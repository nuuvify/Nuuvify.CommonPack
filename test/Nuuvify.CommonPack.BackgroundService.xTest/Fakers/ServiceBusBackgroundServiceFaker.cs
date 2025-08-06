using Azure.Messaging.ServiceBus;
using Bogus;
using Moq;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService.xTest.Fakers;

/// <summary>
/// Faker para gerar dados de teste para ServiceBusBackgroundService
/// </summary>
public static class ServiceBusBackgroundServiceFaker
{
    /// <summary>
    /// Gera uma instância de configuração mock para testes
    /// </summary>
    /// <returns>Mock de IConfigurationCustom configurado</returns>
    public static Mock<IConfigurationCustom> GenerateConfigurationMock()
    {
        var mockConfiguration = new Mock<IConfigurationCustom>();

        _ = mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:CnnName"))
            .Returns("TestConnection");
        _ = mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Topic:Name"))
            .Returns("test-topic");
        _ = mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:Topic:Subscription"))
            .Returns("test-subscription");
        _ = mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:QueueName"))
            .Returns("test-queue");
        _ = mockConfiguration.Setup(x => x.GetSectionValue("ServiceBus:FullyQualifiedNamespace"))
            .Returns("test.servicebus.windows.net");
        _ = mockConfiguration.Setup(x => x.GetConnectionString("TestConnection"))
            .Returns("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=testkey");

        return mockConfiguration;
    }

    /// <summary>
    /// Gera uma RequestConfiguration para testes
    /// </summary>
    /// <returns>Instância de RequestConfiguration</returns>
    public static RequestConfiguration GenerateRequestConfiguration()
    {
        var faker = new Faker();
        return new RequestConfiguration
        {
            CorrelationId = faker.Random.Guid().ToString()
        };
    }

    /// <summary>
    /// Gera uma collection de RequestConfiguration para testes
    /// </summary>
    /// <param name="count">Quantidade de configurações a serem geradas</param>
    /// <returns>Coleção de RequestConfiguration</returns>
    public static IEnumerable<RequestConfiguration> GenerateRequestConfigurations(int count)
    {
        var faker = new Faker<RequestConfiguration>()
            .RuleFor(r => r.CorrelationId, f => f.Random.Guid().ToString());

        return faker.Generate(count);
    }

    /// <summary>
    /// Gera um ActivitySource para testes
    /// </summary>
    /// <returns>Instância de ActivitySource</returns>
    public static ActivitySource GenerateActivitySource()
    {
        var faker = new Faker();
        return new ActivitySource($"Test-{faker.Random.AlphaNumeric(8)}");
    }

    /// <summary>
    /// Gera uma collection de ActivitySource para testes
    /// </summary>
    /// <param name="count">Quantidade de ActivitySource a serem gerados</param>
    /// <returns>Coleção de ActivitySource</returns>
    public static IEnumerable<ActivitySource> GenerateActivitySources(int count)
    {
        var faker = new Faker();
        return Enumerable.Range(0, count)
            .Select(_ => new ActivitySource($"Test-{faker.Random.AlphaNumeric(8)}"));
    }

    /// <summary>
    /// Gera um mock de ServiceBusReceivedMessage para testes
    /// </summary>
    /// <returns>Mock de ServiceBusReceivedMessage</returns>
    public static Mock<ServiceBusReceivedMessage> GenerateServiceBusReceivedMessageMock()
    {
        var faker = new Faker();
        var mockMessage = new Mock<ServiceBusReceivedMessage>();

        _ = mockMessage.Setup(m => m.MessageId).Returns(faker.Random.Guid().ToString());
        _ = mockMessage.Setup(m => m.Subject).Returns(faker.Lorem.Word());
        _ = mockMessage.Setup(m => m.Body).Returns(BinaryData.FromString(faker.Lorem.Sentence()));

        return mockMessage;
    }

    /// <summary>
    /// Gera uma collection de mocks de ServiceBusReceivedMessage para testes
    /// </summary>
    /// <param name="count">Quantidade de mocks a serem gerados</param>
    /// <returns>Coleção de mocks de ServiceBusReceivedMessage</returns>
    public static IEnumerable<Mock<ServiceBusReceivedMessage>> GenerateServiceBusReceivedMessageMocks(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => GenerateServiceBusReceivedMessageMock());
    }
}
