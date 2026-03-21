namespace Nuuvify.CommonPack.AzureServiceBus.Services.Receiver;

/// <summary>
/// Classe base para recebimento de mensagens do Azure Service Bus
/// Esta implementação é agnóstica ao tipo de projeto (API, Worker Service, Console App, etc.)
/// </summary>
/// <typeparam name="T">Tipo que representa o contexto da mensagem processada</typeparam>
public abstract partial class ServiceBusMessageReceiver<T> : IServiceBusMessageReceiver<T>
{
    #region Constants e Fields

    private const string UnknownValue = "Unknown";

    private readonly ILogger<ServiceBusMessageReceiver<T>> _logger;
    private readonly IConfigurationCustom _configurationCustom;
    private readonly RequestConfiguration _requestConfiguration;

    // Recursos IDisposable adequadamente liberados via DisposeAsync
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private ServiceBusClient _serviceBusClient;
    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
    private ServiceBusProcessor _serviceBusProcessor;

    private bool _isProcessing = false;
    private readonly object _lockObject = new();
    private ServiceBusReceiveMode _receiveMode = ServiceBusReceiveMode.PeekLock;

    #endregion

    #region Properties

    protected RequestConfiguration RequestConfiguration => _requestConfiguration;
    protected ILogger<ServiceBusMessageReceiver<T>> Logger => _logger;
    protected IConfigurationCustom ConfigurationCustom => _configurationCustom;
    protected ActivitySource ActivitySourceCustom { get; set; }

    /// <summary>
    /// Modo de recebimento configurado para o processador
    /// </summary>
    protected ServiceBusReceiveMode ReceiveMode => _receiveMode;

    /// <summary>
    /// Configurar para abandonar mensagens em caso de falha em vez de enviá-las para dead letter
    /// </summary>
    protected bool AbandonMessageIfFailed { get; set; } = false;

    /// <summary>
    /// Indica se o processamento está ativo
    /// </summary>
    public bool IsProcessing
    {
        get
        {
            lock (_lockObject)
            {
                return _isProcessing;
            }
        }
    }

    #endregion

    #region Constructor

    protected ServiceBusMessageReceiver(
        ILogger<ServiceBusMessageReceiver<T>> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationCustom = configurationCustom ?? throw new ArgumentNullException(nameof(configurationCustom));
        _requestConfiguration = requestConfiguration ?? throw new ArgumentNullException(nameof(requestConfiguration));

        if (string.IsNullOrEmpty(_requestConfiguration.CorrelationId))
        {
            _requestConfiguration.CorrelationId = Guid.NewGuid().ToString();
        }
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Método abstrato que deve ser implementado pelas classes derivadas para executar a lógica de negócio
    /// Utiliza o RequestConfiguration.CorrelationId como uma das propriedades
    /// </summary>
    /// <param name="message">Mensagem recebida do Service Bus</param>
    /// <param name="activitySource">Source para criação de atividades de telemetria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o processamento foi bem-sucedido, false caso contrário</returns>
    public abstract Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken);

    #endregion

    #region Main Processing Methods

    /// <summary>
    /// Inicia o processamento das mensagens do Service Bus
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento para parar o processamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <exception cref="InvalidOperationException">Quando ServiceBus não foi configurado ou já está processando</exception>
    public virtual async Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (_isProcessing)
            {
                throw new InvalidOperationException("O processamento já está em andamento. Pare o processamento atual antes de iniciar um novo.");
            }

            if (_serviceBusProcessor == null)
            {
                throw new InvalidOperationException("ServiceBus não foi configurado. Chame um dos métodos ConfigureServiceBus antes de iniciar o processamento.");
            }

            _isProcessing = true;
        }

        _logger.LogInformation("Iniciando o processamento das mensagens do Service Bus");

        try
        {
            _serviceBusProcessor.ProcessMessageAsync += (args) => HandleMessageAsync(args, cancellationToken);
            _serviceBusProcessor.ProcessErrorAsync += HandleErrorAsync;

            await _serviceBusProcessor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Processamento de mensagens do Service Bus iniciado com sucesso");
        }
        catch (ServiceBusException ex)
        {
            lock (_lockObject)
            {
                _isProcessing = false;
            }

            _logger.LogError(ex, "Erro específico do Service Bus ao inicializar o processamento: {Reason}", ex.Reason);
            throw new InvalidOperationException($"Falha ao inicializar o processamento do Service Bus: {ex.Reason}", ex);
        }
        catch (Exception ex)
        {
            lock (_lockObject)
            {
                _isProcessing = false;
            }

            _logger.LogError(ex, "Erro inesperado ao inicializar o processamento do Service Bus");
            throw;
        }
    }

    /// <summary>
    /// Para o processamento das mensagens e libera os recursos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public virtual async Task StopProcessingAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_isProcessing)
            {
                _logger.LogInformation("O processamento não está ativo, nenhuma ação necessária");
                return;
            }
        }

        _logger.LogInformation("Parando o processamento das mensagens do Service Bus");

        try
        {
            if (_serviceBusProcessor != null)
            {
                await _serviceBusProcessor.StopProcessingAsync(cancellationToken);
                _logger.LogInformation("Processamento de mensagens do Service Bus parado com sucesso");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar o processamento das mensagens do Service Bus");
            throw new InvalidOperationException($"Erro ao parar o processamento do Service Bus: {ex.Message}", ex);
        }
        finally
        {
            lock (_lockObject)
            {
                _isProcessing = false;
            }
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
        using var activity = ActivitySourceCustom?.StartActivity(nameof(HandleMessageAsync));
        try
        {
            _ = activity?.SetTag("ServiceBus.CorrelationId", RequestConfiguration.CorrelationId);
            _ = activity?.SetTag("ServiceBus.MessageId", args.Message.MessageId);

            _logger.LogInformation("Iniciando processamento da mensagem {MessageId}: {Data}",
                args.Message.MessageId, DateTimeOffset.Now);

            var result = await ExecuteReceivedMessageAsync(args.Message, ActivitySourceCustom, cancellationToken);

            _logger.LogInformation("Finalizando processamento da mensagem {MessageId}: {Data}",
                args.Message.MessageId, DateTimeOffset.Now);

            if (result)
            {
                if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
                {
                    await args.CompleteMessageAsync(args.Message, cancellationToken);
                }

                _logger.LogDebug("Mensagem {MessageId} processada com sucesso", args.Message.MessageId);
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

    #endregion

    #region IAsyncDisposable Implementation

    /// <summary>
    /// Libera os recursos utilizados pela classe
    /// </summary>
    /// <returns>Task representando a operação assíncrona de liberação de recursos</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Método protegido para liberação de recursos de forma assíncrona
    /// </summary>
    /// <returns>Task representando a operação assíncrona de liberação de recursos</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_isProcessing)
        {
            await StopProcessingAsync();
        }

        try
        {
            if (_serviceBusProcessor != null)
            {
                await _serviceBusProcessor.DisposeAsync();
            }

            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync();
            }

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

    #endregion

    #region Helper Methods

    private static void ValidateConfiguration(string value, string parameterName, string errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException(errorMessage, parameterName);
        }
    }

    #endregion
}
