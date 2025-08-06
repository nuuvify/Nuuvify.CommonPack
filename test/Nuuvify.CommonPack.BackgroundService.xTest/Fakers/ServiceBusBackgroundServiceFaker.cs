using Azure.Core;
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
    /// Gera um mock de TokenCredential para testes
    /// </summary>
    /// <returns>Mock de TokenCredential</returns>
    public static Mock<TokenCredential> GenerateTokenCredentialMock()
    {
        var mockCredential = new Mock<TokenCredential>();
        return mockCredential;
    }

    /// <summary>
    /// Gera um mock de ServiceBusReceiver para testes
    /// </summary>
    /// <returns>Mock de ServiceBusReceiver</returns>
    public static Mock<ServiceBusReceiver> GenerateServiceBusReceiverMock()
    {
        var mockReceiver = new Mock<ServiceBusReceiver>();
        return mockReceiver;
    }

    /// <summary>
    /// Gera um mock de ProcessMessageEventArgs para testes
    /// </summary>
    /// <param name="receiver">Mock do ServiceBusReceiver</param>
    /// <param name="abandon">Se deve abandonar a mensagem em caso de falha</param>
    /// <returns>Mock de ProcessMessageEventArgs</returns>
    public static Mock<ProcessMessageEventArgs> GenerateProcessMessageEventArgsMock(ServiceBusReceiver receiver, bool abandon = false)
    {
        var mockArgs = new Mock<ProcessMessageEventArgs>();

        // Como ServiceBusReceivedMessage é sealed, não podemos mocká-lo
        // Vamos apenas configurar os métodos que são chamados nos testes
        _ = mockArgs.Setup(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _ = mockArgs.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _ = mockArgs.Setup(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mockArgs;
    }

    /// <summary>
    /// Gera um mock de ProcessErrorEventArgs para testes
    /// Nota: Esta classe é sealed, então não pode ser mockada diretamente
    /// </summary>
    /// <returns>Instância real de ProcessErrorEventArgs (pode ser null nos testes)</returns>
    public static ProcessErrorEventArgs? GenerateProcessErrorEventArgsReal()
    {
        // ProcessErrorEventArgs é sealed e não pode ser mockado
        // Em testes reais, retornaríamos null ou usaríamos reflection para criar instâncias
        return null;
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
}
