namespace Nuuvify.CommonPack.AzureServiceBus.Services.Receiver;

/// <summary>
/// Implementação do Azure Service Bus - Métodos de Configuração
/// </summary>
public abstract partial class ServiceBusMessageReceiver<T>
{
    #region Configuration Methods

    /// <summary>
    /// Configura o Service Bus com string de conexão para Topic
    /// </summary>
    /// <param name="cnnName">ServiceBus:SuaAplicacao:ConnectionString (Nome da propriedade onde esta a ConnectionString no Vault)</param>
    /// <param name="topicName">ServiceBus:SuaAplicacao:TopicName</param>
    /// <param name="subscription">ServiceBus:SuaAplicacao:TopicSubscription</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    /// <example>
    /// Exemplo de configuração no appsettings.json:
    /// <code>
    ///    "ServiceBus:Reinf": {
    ///        "TopicName": "topic-reinf",
    ///        "TopicSubscription": "Worker_InterfacePagamentos",
    ///        "QueueName": "queue-reinf",
    ///        "OperationTimeoutSeconds": 30,
    ///        "MaxRetryAttempts": 3,
    ///        "RetryDelaySeconds": 5,
    ///        "MaxBatchSize": 100,
    ///        "DefaultMessageTtlMinutes": 60,
    ///        "ConnectionString": "Endpoint=sb://teste"
    ///    },
    /// </code>
    /// </example>
    protected virtual void ConfigureServiceBus(
        string cnnName,
        string topicName,
        string subscription,
        ServiceBusClientOptions serviceBusClientOptions = null,
        ServiceBusProcessorOptions serviceBusProcessorOptions = null)
    {
        ValidateConfiguration(cnnName, nameof(cnnName), "A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString'");
        ValidateConfiguration(topicName, nameof(topicName), "O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicName'");
        ValidateConfiguration(subscription, nameof(subscription), "A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicSubscription'");

        var connectionString = _configurationCustom.GetSectionValue(cnnName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ServiceBus:SuaAplicacao:ConnectionString do Vault. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions ?? new ServiceBusClientOptions());

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            topicName: topicName,
            subscriptionName: subscription,
            options: serviceBusProcessorOptions ?? new ServiceBusProcessorOptions());
    }

    /// <summary>
    /// Configura o Service Bus com credenciais Azure para Topic
    /// </summary>
    /// <param name="topicName">ServiceBus:SuaAplicacao:TopicName</param>
    /// <param name="subscription">ServiceBus:SuaAplicacao:TopicSubscription</param>
    /// <param name="fullyQualifiedNamespace">ServiceBus:SuaAplicacao:FullyQualifiedNamespace</param>
    /// <param name="credential">Credencial Azure para autenticação</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string topicName,
        string subscription,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        ServiceBusClientOptions serviceBusClientOptions = null,
        ServiceBusProcessorOptions serviceBusProcessorOptions = null)
    {
        ValidateConfiguration(topicName, nameof(topicName), "O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicName'");
        ValidateConfiguration(subscription, nameof(subscription), "A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicSubscription'");
        ValidateConfiguration(fullyQualifiedNamespace, nameof(fullyQualifiedNamespace), "O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:FullyQualifiedNamespace'");

        if (credential == null)
        {
            throw new ArgumentException("O TokenCredential do Azure não foi configurado corretamente. Verifique a configuração 'AzureCredentialBuilderExtensions'");
        }

        _serviceBusClient = new ServiceBusClient(
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            credential: credential,
            options: serviceBusClientOptions ?? new ServiceBusClientOptions());

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            topicName: topicName,
            subscriptionName: subscription,
            options: serviceBusProcessorOptions ?? new ServiceBusProcessorOptions());
    }

    /// <summary>
    /// Configura o Service Bus com string de conexão para Queue
    /// </summary>
    /// <param name="cnnName">ServiceBus:SuaAplicacao:ConnectionString (Nome da ConnectionString no Vault)</param>
    /// <param name="queueName">ServiceBus:SuaAplicacao:QueueName</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string cnnName,
        string queueName,
        ServiceBusClientOptions serviceBusClientOptions = null,
        ServiceBusProcessorOptions serviceBusProcessorOptions = null)
    {
        ValidateConfiguration(cnnName, nameof(cnnName), "A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString'");
        ValidateConfiguration(queueName, nameof(queueName), "A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");

        var connectionString = _configurationCustom.GetSectionValue(cnnName);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ConnectionString do Vault. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions ?? new ServiceBusClientOptions());

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            queueName: queueName,
            options: serviceBusProcessorOptions ?? new ServiceBusProcessorOptions());
    }

    /// <summary>
    /// Configura o Service Bus com credenciais Azure para Queue
    /// </summary>
    /// <param name="queueName">ServiceBus:SuaAplicacao:QueueName</param>
    /// <param name="fullyQualifiedNamespace">ServiceBus:SuaAplicacao:FullyQualifiedNamespace</param>
    /// <param name="credential">Credencial Azure para autenticação</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string queueName,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        ServiceBusClientOptions serviceBusClientOptions = null,
        ServiceBusProcessorOptions serviceBusProcessorOptions = null)
    {
        ValidateConfiguration(queueName, nameof(queueName), "A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");
        ValidateConfiguration(fullyQualifiedNamespace, nameof(fullyQualifiedNamespace), "O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:FullyQualifiedNamespace'");

        if (credential == null)
        {
            throw new ArgumentException("O TokenCredential do Azure não foi configurado corretamente. Verifique a configuração 'AzureCredentialBuilderExtensions'");
        }

        _serviceBusClient = new ServiceBusClient(
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            credential: credential,
            options: serviceBusClientOptions ?? new ServiceBusClientOptions());

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            queueName: queueName,
            options: serviceBusProcessorOptions ?? new ServiceBusProcessorOptions());
    }

    #endregion
}
