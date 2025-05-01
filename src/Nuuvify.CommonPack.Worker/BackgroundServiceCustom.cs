using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Net;

namespace Nuuvify.CommonPack.Worker;

public partial class BackgroundServiceCustom<T> : BackgroundService
{
    private readonly ILogger<BackgroundServiceCustom<T>> _logger;
    private readonly TelemetryClient _telemetryClient;
    private readonly IConfigurationCustom _configurationCustom;
    private readonly RequestConfiguration _requestConfiguration;

    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusProcessor _serviceBusProcessor;

    public RequestConfiguration RequestConfiguration => _requestConfiguration;
    public ILogger<BackgroundServiceCustom<T>> Logger => _logger;
    public IConfigurationCustom ConfigurationCustom => _configurationCustom;

    public BackgroundServiceCustom(ILogger<BackgroundServiceCustom<T>> logger, TelemetryClient telemetryClient, IConfigurationCustom configurationCustom, RequestConfiguration requestConfiguration, int maxConcurrentCalls, TimeSpan maxAutoLockRenewalDuration)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _configurationCustom = configurationCustom;
        _requestConfiguration = requestConfiguration;

        _requestConfiguration.CorrelationId ??= Guid.NewGuid().ToString();

        var busCnn = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Cnn");
        var topic = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Topic");
        var subscription = _configurationCustom.GetSectionValue("AppConfig:ServiceBus:Subscription");

        _serviceBusClient = new ServiceBusClient(_configurationCustom.GetConnectionString(busCnn), new ServiceBusClientOptions
        {
            WebProxy = WebRequest.DefaultWebProxy,
            TransportType = ServiceBusTransportType.AmqpWebSockets
        });

        var serviceBusProcessorOption = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = maxConcurrentCalls,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = maxAutoLockRenewalDuration
        };

        _serviceBusProcessor = _serviceBusClient.CreateProcessor(topic, subscription, serviceBusProcessorOption);
    }

    public virtual Task<bool> ExecuteRule(ServiceBusReceivedMessage message, TelemetryClient tc)
    {
        // Implementar a lógica de execução da regra aqui
        // Exemplo: Processar a mensagem recebida e retornar um resultado
        return Task.FromResult(true);
    }

    #region Bus Processor

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
        try
        {
            using var op = _telemetryClient.StartOperation<RequestTelemetry>($"{_requestConfiguration.AppName}-{nameof(ExecuteAsync)}");
            _logger.LogInformation("Iniciando ciclo Worker: {Data}", DateTimeOffset.Now);

            var result = await ExecuteRule(args.Message, _telemetryClient);

            _logger.LogInformation("Finalizando ciclo Worker: {Result}", result);
            _telemetryClient.StopOperation(op);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Houve um erro durante a execução do Worker.ExecuteAsync");
            await args.DeadLetterMessageAsync(args.Message);
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

    #endregion

    #region Dispose

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

    #endregion
}
