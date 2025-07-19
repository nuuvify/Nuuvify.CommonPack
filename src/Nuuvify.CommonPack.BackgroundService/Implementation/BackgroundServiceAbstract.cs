using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using System.Net;

namespace Nuuvify.CommonPack.BackgroundServiceAbstract;

public abstract partial class BackgroundServiceCustom<T> : BackgroundService
{
    private readonly ILogger<BackgroundServiceCustom<T>> _logger;
    private readonly IConfigurationCustom _configurationCustom;
    private readonly RequestConfiguration _requestConfiguration;

    private ServiceBusClient _serviceBusClient;
    private ServiceBusProcessor _serviceBusProcessor;

    protected RequestConfiguration RequestConfiguration => _requestConfiguration;
    protected ILogger<BackgroundServiceCustom<T>> Logger => _logger;
    protected IConfigurationCustom ConfigurationCustom => _configurationCustom;
    protected static ActivitySource ActivitySourceCustom { get; set; }
    protected bool AbandonMessageIfFailed { get; set; } = false;

    protected BackgroundServiceCustom(
        ILogger<BackgroundServiceCustom<T>> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        int maxConcurrentCalls,
        TimeSpan maxAutoLockRenewalDuration)
    {
        _logger = logger;
        _configurationCustom = configurationCustom;
        _requestConfiguration = requestConfiguration;

        _requestConfiguration.CorrelationId ??= Guid.NewGuid().ToString();

        var serviceBusClientOptions = new ServiceBusClientOptions
        {
            WebProxy = WebRequest.DefaultWebProxy,
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        var serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = maxConcurrentCalls,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = maxAutoLockRenewalDuration
        };
        ConfigureServiceBus(serviceBusClientOptions, serviceBusProcessorOptions);

    }

    public virtual void ConfigureServiceBus(ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        var busCnn = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Cnn");
        var topic = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Topic");
        var subscription = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Subscription");

        var connectionString = _configurationCustom.GetConnectionString(busCnn);

        if (string.IsNullOrEmpty(busCnn))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'AppConfig:ServiceBus:Cnn'");
        }
        if (string.IsNullOrEmpty(subscription))
        {
            throw new ArgumentException("A assinatura do Service Bus não foi configurada corretamente. Verifique a configuração 'AppConfig:ServiceBus:Subscription'");
        }
        if (string.IsNullOrEmpty(topic))
        {
            throw new ArgumentException("O tópico do Service Bus não foi configurado corretamente. Verifique a configuração 'AppConfig:ServiceBus:Topic'");
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'AppConfig:ServiceBus:Cnn'");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString,
            serviceBusClientOptions);

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(topic, subscription, serviceBusProcessorOptions);
    }

    public virtual Task<bool> ExecuteRule(ServiceBusReceivedMessage message, ActivitySource activitySource)
    {
        // Implementar a lógica de execução da regra aqui
        // Exemplo: Processar a mensagem recebida e retornar um resultado
        return Task.FromResult(true);
    }

    /// <summary>
    /// Executes the background service asynchronously, initializing and starting the Service Bus message processing.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to stop the execution of the background service.</param>
    /// <returns>A task that represents the asynchronous execution of the background service.</returns>
    /// <remarks>
    /// This method sets up event handlers for message processing and error handling, then starts the Service Bus processor.
    /// Any exceptions during initialization are logged as errors.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o processamento das mensagens do bus");

        try
        {
            _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;

            await _serviceBusProcessor.StartProcessingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Houve um erro ao inicializar o processamento das mensagens do bus");
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (ActivitySourceCustom == null)
        {
            throw new ArgumentException("ActivitySourceCustom não está configurado. Certifique-se de que o ActivitySource foi inicializado corretamente.");
        }

        using (var activity = ActivitySourceCustom.StartActivity("WorkerLoop"))
        {
            try
            {
                _ = (activity?.SetTag("Worker.CorrelationId", _requestConfiguration.CorrelationId));

                _logger.LogInformation("Iniciando ciclo Worker: {Data}", DateTimeOffset.Now);

                var result = await ExecuteRule(args.Message, ActivitySourceCustom);

                _logger.LogInformation("Finalizando ciclo Worker: {Result}", result);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (ServiceBusException ex) when (
                ex.Reason == ServiceBusFailureReason.MessageLockLost ||
                ex.Reason == ServiceBusFailureReason.SessionLockLost ||
                ex.Reason == ServiceBusFailureReason.QuotaExceeded)
            {
                _logger.LogError(ex, "Erro específico do Service Bus durante a execução do Worker: {Reason}", ex.Reason);
                await args.DeadLetterMessageAsync(args.Message);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Operação cancelada durante a execução do Worker");
                if (AbandonMessageIfFailed)
                {
                    await args.AbandonMessageAsync(args.Message);
                }
                else
                {
                    await args.DeadLetterMessageAsync(args.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Houve um erro durante a execução do Worker.ExecuteAsync");
                await args.DeadLetterMessageAsync(args.Message);
                throw;
            }
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Erro ao processar a mensagem");

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
        await _serviceBusProcessor.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }

    public override void Dispose()
    {
        Dispose(true);
        base.Dispose();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        _serviceBusClient.DisposeAsync().AsTask().Wait();
        _serviceBusProcessor.DisposeAsync().AsTask().Wait();
    }

}
