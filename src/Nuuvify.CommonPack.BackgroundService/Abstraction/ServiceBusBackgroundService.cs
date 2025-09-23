using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService.Abstraction;

public abstract partial class ServiceBusBackgroundService<T> : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly ILogger<ServiceBusBackgroundService<T>> _logger;
    private readonly IConfigurationCustom _configurationCustom;
    private readonly RequestConfiguration _requestConfiguration;

    private ServiceBusClient _serviceBusClient;
    private ServiceBusProcessor _serviceBusProcessor;

    protected RequestConfiguration RequestConfiguration => _requestConfiguration;
    protected ILogger<ServiceBusBackgroundService<T>> Logger => _logger;
    protected IConfigurationCustom ConfigurationCustom => _configurationCustom;
    protected ActivitySource ActivitySourceCustom { get; set; }

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
    /// <param name="cnnName">ServiceBus:CnnName (Nome da ConnectionString no Vault)</param>
    /// <param name="topicName">ServiceBus:Topic:Name</param>
    /// <param name="subscription">ServiceBus:Topic:Subscription</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string cnnName,
        string topicName,
        string subscription,
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        var connectionString = _configurationCustom.GetConnectionString(cnnName);

        if (string.IsNullOrEmpty(cnnName))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:CnnName'");
        }
        if (string.IsNullOrEmpty(subscription))
        {
            throw new ArgumentException("A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:Topic:Subscription'");
        }
        if (string.IsNullOrEmpty(topicName))
        {
            throw new ArgumentException("O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:Topic:Name'");
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ConnectionString do Vault. Verifique a configuração 'ServiceBus:CnnName' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            topicName: topicName,
            subscriptionName: subscription,
            options: serviceBusProcessorOptions);
    }

    /// <summary>
    /// Configura o Service Bus com credenciais Azure
    /// </summary>
    /// <param name="topicName">ServiceBus:Topic:Name</param>
    /// <param name="subscription">ServiceBus:Topic:Subscription</param>
    /// <param name="fullyQualifiedNamespace">ServiceBus:FullyQualifiedNamespace</param>
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
            throw new ArgumentException("A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:Topic:Subscription'");
        }
        if (string.IsNullOrEmpty(topicName))
        {
            throw new ArgumentException("O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:Topic:Name'");
        }
        if (string.IsNullOrEmpty(fullyQualifiedNamespace))
        {
            throw new ArgumentException("O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:FullyQualifiedNamespace'");
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
    }

    /// <summary>
    /// Configura o Service Bus com string de conexão
    /// </summary>
    /// <param name="cnnName">ServiceBus:CnnName (Nome da ConnectionString no Vault)</param>
    /// <param name="queueName">ServiceBus:QueueName</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusProcessorOptions">Opções do processador Service Bus</param>
    protected virtual void ConfigureServiceBus(
        string cnnName,
        string queueName,
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        var connectionString = _configurationCustom.GetConnectionString(cnnName);

        if (string.IsNullOrEmpty(cnnName))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:CnnName'");
        }
        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:QueueName'");
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ConnectionString do Vault. Verifique a configuração 'ServiceBus:CnnName' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(
            queueName: queueName,
            options: serviceBusProcessorOptions);
    }

    /// <summary>
    /// Configura o Service Bus com string de conexão
    /// </summary>
    /// <param name="queueName">ServiceBus:QueueName</param>
    /// <param name="fullyQualifiedNamespace">ServiceBus:FullyQualifiedNamespace</param>
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
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:QueueName'");
        }
        if (string.IsNullOrEmpty(fullyQualifiedNamespace))
        {
            throw new ArgumentException("O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:FullyQualifiedNamespace'");
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
    }

    /// <summary>
    /// Método abstrato que deve ser implementado pelas classes derivadas para executar a lógica de negócio
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
    /// Método principal que executa o processamento das mensagens do Service Bus
    /// </summary>
    /// <param name="stoppingToken">Token de cancelamento para parar o serviço</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o processamento das mensagens do bus");

        try
        {

            _serviceBusProcessor.ProcessMessageAsync += (args) => HandleMessageAsync(args, stoppingToken);
            _serviceBusProcessor.ProcessErrorAsync += HandleErrorAsync;

            await _serviceBusProcessor.StartProcessingAsync(stoppingToken);

            // Aguarda até que o token de cancelamento seja acionado
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processamento de mensagens foi cancelado");
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Erro específico do Service Bus ao inicializar o processamento: {Reason}", ex.Reason);
            throw;
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
                await args.CompleteMessageAsync(args.Message, cancellationToken);
            }
            else
            {
                _logger.LogWarning("{MethodName} retornou false para a mensagem {MessageId}. Verificando comportamento de falha.",
                    nameof(ExecuteReceivedMessageAsync), args.Message.MessageId);

                if (AbandonMessageIfFailed)
                {
                    await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
                }
                else
                {
                    await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
                }
            }
        }
        catch (ServiceBusException ex) when (
            ex.Reason == ServiceBusFailureReason.MessageLockLost ||
            ex.Reason == ServiceBusFailureReason.SessionLockLost ||
            ex.Reason == ServiceBusFailureReason.QuotaExceeded)
        {
            _logger.LogError(ex, "Erro específico do Service Bus durante a execução do Worker: {Reason}", ex.Reason);
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
        {
            _logger.LogError(ex, "Erro de comunicação no Service Bus. Verifique as configurações de rede. Reason: {Reason}, Resource: {EntityPath}",
                ex.Reason, args.Message?.Subject ?? "Unknown");
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operação cancelada durante a execução do Worker");
            if (AbandonMessageIfFailed)
            {
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            else
            {
                await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Houve um erro durante a execução do Worker.ExecuteAsync");
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
            throw;
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
            throw;
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
        Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Método protegido para liberação de recursos
    /// </summary>
    /// <param name="disposing">Indica se o método está sendo chamado pelo Dispose ou pelo finalizador</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                _serviceBusProcessor?.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                _serviceBusClient?.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
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
