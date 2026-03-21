using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Nuuvify.CommonPack.BackgroundService.Services;

public abstract partial class ServiceBusBackgroundService<T> : Microsoft.Extensions.Hosting.BackgroundService
{
    private const string UnknownValue = "Unknown";

    private readonly ILogger<ServiceBusBackgroundService<T>> _logger;
    private readonly IConfigurationCustom _configurationCustom;
    private readonly RequestConfiguration _requestConfiguration;

    // Recursos IDisposable adequadamente liberados via DisposeAsync no método DisposeCustom
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private ServiceBusClient _serviceBusClient;
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private ServiceBusProcessor _serviceBusProcessor;
    private ServiceBusReceiveMode _receiveMode = ServiceBusReceiveMode.PeekLock;

    protected RequestConfiguration RequestConfiguration => _requestConfiguration;
    protected ILogger<ServiceBusBackgroundService<T>> Logger => _logger;
    protected IConfigurationCustom ConfigurationCustom => _configurationCustom;
    protected ActivitySource ActivitySourceCustom { get; set; } = null!;

    /// <summary>
    /// Modo de recebimento configurado para o processador
    /// </summary>
    protected ServiceBusReceiveMode ReceiveMode => _receiveMode;

    /// <summary>
    /// Configurar para abandonar mensagens em caso de falha em vez de enviá-las para dead letter
    /// </summary>
    protected bool AbandonMessageIfFailed { get; set; } = false;

    protected ServiceBusBackgroundService(
        ILogger<ServiceBusBackgroundService<T>> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
    {
        _logger = logger;
        _configurationCustom = configurationCustom;
        _requestConfiguration = requestConfiguration;

        if (string.IsNullOrEmpty(_requestConfiguration.CorrelationId))
        {
            _requestConfiguration.CorrelationId = Guid.NewGuid().ToString();
        }

    }

    /// <summary>
    /// Configura o Service Bus com string de conexão
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
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        var connectionString = _configurationCustom.GetSectionValue(cnnName);

        if (string.IsNullOrEmpty(cnnName))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString'");
        }
        if (string.IsNullOrEmpty(subscription))
        {
            throw new ArgumentException("A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicSubscription'");
        }
        if (string.IsNullOrEmpty(topicName))
        {
            throw new ArgumentException("O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicName'");
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ServiceBus:SuaAplicacao:ConnectionString do Vault. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            topicName: topicName,
            subscriptionName: subscription,
            options: serviceBusProcessorOptions);

        _receiveMode = serviceBusProcessorOptions?.ReceiveMode ?? ServiceBusReceiveMode.PeekLock;
    }

    /// <summary>
    /// Configura o Service Bus com credenciais Azure
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
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {

        if (string.IsNullOrEmpty(subscription))
        {
            throw new ArgumentException("A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicSubscription'");
        }
        if (string.IsNullOrEmpty(topicName))
        {
            throw new ArgumentException("O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:TopicName'");
        }
        if (string.IsNullOrEmpty(fullyQualifiedNamespace))
        {
            throw new ArgumentException("O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:FullyQualifiedNamespace'");
        }
        if (credential == null)
        {
            throw new ArgumentException("O TokenCredential do Azure não foi configurado corretamente. Verifique a configuração 'AzureCredentialBuilderExtensions'");
        }

        _serviceBusClient = new ServiceBusClient(
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            credential: credential,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            topicName: topicName,
            subscriptionName: subscription,
            options: serviceBusProcessorOptions);

        _receiveMode = serviceBusProcessorOptions?.ReceiveMode ?? ServiceBusReceiveMode.PeekLock;
    }

    /// <summary>
    /// Configura o Service Bus com string de conexão
    /// </summary>
    /// <param name="cnnName">ServiceBus:SuaAplicacao:ConnectionString (Nome da ConnectionString no Vault)</param>
    /// <param name="queueName">ServiceBus:SuaAplicacao:QueueName</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string cnnName,
        string queueName,
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        var connectionString = _configurationCustom.GetSectionValue(cnnName);

        if (string.IsNullOrEmpty(cnnName))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString'");
        }
        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ConnectionString do Vault. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            queueName: queueName,
            options: serviceBusProcessorOptions);

        _receiveMode = serviceBusProcessorOptions?.ReceiveMode ?? ServiceBusReceiveMode.PeekLock;
    }

    /// <summary>
    /// Configura o Service Bus com string de conexão
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
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {

        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");
        }
        if (string.IsNullOrEmpty(fullyQualifiedNamespace))
        {
            throw new ArgumentException("O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:FullyQualifiedNamespace'");
        }
        if (credential == null)
        {
            throw new ArgumentException("O TokenCredential do Azure não foi configurado corretamente. Verifique a configuração 'AzureCredentialBuilderExtensions'");
        }

        _serviceBusClient = new ServiceBusClient(
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            credential: credential,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            queueName: queueName,
            options: serviceBusProcessorOptions);

        _receiveMode = serviceBusProcessorOptions?.ReceiveMode ?? ServiceBusReceiveMode.PeekLock;
    }

    /// <summary>
    /// Método abstrato que deve ser implementado pelas classes derivadas para executar a lógica de negócio
    /// Utilisa o RequestConfiguration.CorrelationId como umas das propriedades
    /// </summary>
    /// <param name="message">Mensagem recebida do Service Bus</param>
    /// <param name="activitySource">Source para criação de atividades de telemetria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o processamento foi bem-sucedido, false caso contrário</returns>
    protected abstract Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cria propriedades de diagnóstico para mensagens que vão para Dead Letter Queue
    /// Utilisa o RequestConfiguration.CorrelationId como umas das propriedades
    /// </summary>
    /// <param name="message">Mensagem original</param>
    /// <param name="errorDetails">Detalhes do erro</param>
    /// <param name="exceptionType">Tipo da exceção</param>
    /// <returns>Dicionário com propriedades de diagnóstico</returns>
    /// <remarks>
    /// Propriedades criadas:
    /// <list type="bullet">
    /// <item><description><c>ErrorDetails</c>: Detalhes específicos do erro ocorrido</description></item>
    /// <item><description><c>FailureTime</c>: Timestamp ISO 8601 do momento da falha</description></item>
    /// <item><description><c>WorkerVersion</c>: Versão do assembly do worker</description></item>
    /// <item><description><c>CorrelationId</c>: ID de correlação da requisição</description></item>
    /// <item><description><c>DeliveryAttempt</c>: Número de tentativas de entrega da mensagem</description></item>
    /// <item><description><c>MessageId</c>: ID único da mensagem do Service Bus</description></item>
    /// <item><description><c>ExceptionType</c>: Tipo da exceção ou "ProcessingFailure" se não especificado</description></item>
    /// <item><description><c>WorkerInstance</c>: Nome da máquina onde o worker está executando</description></item>
    /// <item><description><c>ProcessedBy</c>: Nome da classe que processou a mensagem</description></item>
    /// </list>
    /// </remarks>
    protected virtual Dictionary<string, object> CreateDeadLetterProperties(
        ServiceBusReceivedMessage message,
        string errorDetails,
        string exceptionType = null)
    {
        return new Dictionary<string, object>
        {
            ["ErrorDetails"] = errorDetails,
            ["FailureTime"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["WorkerVersion"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? UnknownValue,
            ["CorrelationId"] = RequestConfiguration.CorrelationId,
            ["DeliveryAttempt"] = message.DeliveryCount,
            ["MessageId"] = message.MessageId,
            ["ExceptionType"] = exceptionType ?? "ProcessingFailure",
            ["WorkerInstance"] = Environment.MachineName,
            ["ProcessedBy"] = Assembly.GetEntryAssembly()?.GetName().Name ?? GetType().Name
        };
    }

    /// <summary>
    /// Cria propriedades de diagnóstico para mensagens abandonadas
    /// Utilisa o RequestConfiguration.CorrelationId como umas das propriedades
    /// </summary>
    /// <param name="message">Mensagem original</param>
    /// <param name="abandonReason">Motivo do abandono</param>
    /// <returns>Dicionário com propriedades de diagnóstico</returns>
    /// <remarks>
    /// Propriedades criadas:
    /// <list type="bullet">
    /// <item><description><c>AbandonReason</c>: Motivo específico pelo qual a mensagem foi abandonada</description></item>
    /// <item><description><c>AbandonTime</c>: Timestamp ISO 8601 do momento do abandono</description></item>
    /// <item><description><c>RetryCount</c>: Número de tentativas já realizadas (DeliveryCount)</description></item>
    /// <item><description><c>CorrelationId</c>: ID de correlação da requisição</description></item>
    /// <item><description><c>MessageId</c>: ID único da mensagem do Service Bus</description></item>
    /// <item><description><c>WorkerInstance</c>: Nome da máquina onde o worker está executando</description></item>
    /// <item><description><c>NextRetryHint</c>: Sugestão de timestamp para próxima tentativa (1 minuto após o abandono)</description></item>
    /// <item><description><c>ProcessedBy</c>: Nome da classe que processou a mensagem</description></item>
    /// </list>
    /// </remarks>
    protected virtual Dictionary<string, object> CreateAbandonProperties(
        ServiceBusReceivedMessage message,
        string abandonReason)
    {
        return new Dictionary<string, object>
        {
            ["AbandonReason"] = abandonReason,
            ["AbandonTime"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["RetryCount"] = message.DeliveryCount,
            ["CorrelationId"] = RequestConfiguration.CorrelationId,
            ["MessageId"] = message.MessageId,
            ["WorkerInstance"] = Environment.MachineName,
            ["NextRetryHint"] = DateTimeOffset.UtcNow.AddMinutes(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["ProcessedBy"] = Assembly.GetEntryAssembly()?.GetName().Name ?? GetType().Name
        };
    }

    /// <summary>
    /// Método principal que executa o processamento das mensagens do Service Bus
    /// </summary>
    /// <param name="stoppingToken">Token de cancelamento para parar o serviço</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o processamento das mensagens do bus");

        try
        {
            if (_serviceBusProcessor == null)
            {
                throw new InvalidOperationException("ServiceBus não foi configurado. Chame um dos métodos ConfigureServiceBus antes de iniciar o processamento.");
            }

            _serviceBusProcessor.ProcessMessageAsync += (args) => HandleMessageAsync(args, stoppingToken);
            _serviceBusProcessor.ProcessErrorAsync += HandleErrorAsync;

            await _serviceBusProcessor.StartProcessingAsync(stoppingToken);

            // Aguarda até que o token de cancelamento seja acionado
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Processamento de mensagens foi cancelado");
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Erro específico do Service Bus ao inicializar o processamento: {Reason}", ex.Reason);
            throw new InvalidOperationException($"Falha ao inicializar o processamento do Service Bus: {ex.Reason}", ex);
        }
        finally
        {
            _logger.LogInformation("Finalizando o processamento das mensagens do bus");
        }
    }

    /// <summary>
    /// Processa uma mensagem individual do Service Bus
    /// </summary>
    /// <param name="args">Argumentos da mensagem a ser processada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected virtual async Task HandleMessageAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
    {
        if (ActivitySourceCustom == null)
        {
            throw new ArgumentException($"{nameof(ActivitySourceCustom)} não está configurado. Certifique-se de que o ActivitySource foi inicializado corretamente.");
        }

        using var activity = ActivitySourceCustom.StartActivity(nameof(HandleMessageAsync));
        try
        {
            _ = (activity?.SetTag("Worker.CorrelationId", RequestConfiguration.CorrelationId));

            _logger.LogInformation("Iniciando {ClassName} Worker: {Data}", nameof(HandleMessageAsync), DateTimeOffset.Now);

            var result = await ExecuteReceivedMessageAsync(args.Message, ActivitySourceCustom, cancellationToken);

            _logger.LogInformation("Finalizando {ClassName} Worker: {Data}", nameof(HandleMessageAsync), DateTimeOffset.Now);

            if (result)
            {
                if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
                {
                    await args.CompleteMessageAsync(args.Message, cancellationToken);
                }
            }
            else
            {
                await HandleBusinessLogicFailureAsync(args, cancellationToken);
            }
        }
        catch (ServiceBusException ex) when (
            ex.Reason == ServiceBusFailureReason.MessageLockLost ||
            ex.Reason == ServiceBusFailureReason.SessionLockLost ||
            ex.Reason == ServiceBusFailureReason.QuotaExceeded)
        {
            await HandleServiceBusSpecificExceptionAsync(args, ex, cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
        {
            await HandleServiceBusCommunicationExceptionAsync(args, ex, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            await HandleOperationCanceledExceptionAsync(args, ex, cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(args, ex, cancellationToken);
        }
    }

    /// <summary>
    /// Processa erros que ocorrem durante o processamento das mensagens
    /// </summary>
    /// <param name="args">Argumentos do erro</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected virtual Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Erro ao processar a mensagem. Source: {Source}, EntityPath: {EntityPath}, FullyQualifiedNamespace: {FullyQualifiedNamespace}",
            args.ErrorSource, args.EntityPath, args.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Para o processamento das mensagens e libera os recursos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando o processamento das mensagens do bus");

        try
        {
            if (_serviceBusProcessor != null)
            {
                await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
                await _serviceBusProcessor.DisposeAsync();
            }

            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar o processamento das mensagens do bus");
            throw new InvalidOperationException($"Erro ao parar o processamento do Service Bus: {ex.Message}", ex);
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Libera os recursos utilizados pela classe
    /// </summary>
    public sealed override void Dispose()
    {
        DisposeCustom(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Método protegido para liberação de recursos
    /// </summary>
    /// <param name="disposing">Indica se o método está sendo chamado pelo Dispose ou pelo finalizador</param>
    protected virtual void DisposeCustom(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Dispose dos recursos IDisposable para satisfazer CA1001
                _serviceBusProcessor?.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                _serviceBusClient?.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

                ActivitySourceCustom?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Ignorar se já foi liberado
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogWarning(ex, "Recurso do Service Bus já estava em processo de liberação");
            }
        }
    }

}
