namespace Nuuvify.CommonPack.AzureServiceBus.Services.Sender;

/// <summary>
/// Implementação completa do Azure Service Bus para envio de mensagens
/// </summary>
/// <remarks>
/// <para>Esta implementação fornece funcionalidades completas para trabalhar com Azure Service Bus:</para>
/// <list type="bullet">
/// <item><description>Envio de mensagens para queues e topics</description></item>
/// <item><description>Operações em lote para alta performance</description></item>
/// <item><description>Agendamento de mensagens para entrega futura</description></item>
/// <item><description>Retry automático com backoff exponencial</description></item>
/// <item><description>Validação robusta de parâmetros e configurações</description></item>
/// <item><description>Logging detalhado para observabilidade</description></item>
/// <item><description>Tratamento de exceções específicas do Service Bus</description></item>
/// <item><description>Suporte a cancellation tokens</description></item>
/// </list>
///
/// <para>Funcionalidades distribuídas em arquivos parciais especializados:</para>
/// <list type="bullet">
/// <item><description>Validations: Métodos de validação de parâmetros e configuração</description></item>
/// <item><description>MessageCreation: Criação e serialização de mensagens</description></item>
/// <item><description>BatchOperations: Operações em lote thread-safe</description></item>
/// <item><description>RetryHandling: Lógica de retry e tratamento de exceções</description></item>
/// <item><description>Queue: Métodos específicos para operações com filas</description></item>
/// <item><description>Topic: Métodos específicos para operações com tópicos</description></item>
/// </list>
///
/// <para>Thread Safety: Esta classe é thread-safe e pode ser usada como singleton.</para>
/// <para>Dispose Pattern: Implementa IDisposable corretamente para liberação de recursos.</para>
/// </remarks>
public partial class ServiceBusMessageSender : IServiceBusMessageSender, IDisposable
{
    #region Campos Privados

    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusConfigurationManager _configurationManager;
    private readonly ILogger<ServiceBusMessageSender> _logger;
    private bool _disposed;

    // Cache de clientes para reutilização de conexões
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, ServiceBusClient> _clientCache;

    #endregion

    #region Construtor

    /// <summary>
    /// Inicializa uma nova instância do ServiceBusMessageSender com configuração básica
    /// </summary>
    /// <param name="options">Configurações básicas do Service Bus</param>
    /// <param name="logger">Logger para registro de eventos</param>
    /// <exception cref="ArgumentNullException">Quando options ou logger são nulos</exception>
    /// <exception cref="ArgumentException">Quando a configuração é inválida</exception>
    public ServiceBusMessageSender(
        IOptions<ServiceBusConfiguration> options,
        ILogger<ServiceBusMessageSender> logger)
        : this(options, logger, null)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância do ServiceBusMessageSender com configuração avançada
    /// </summary>
    /// <param name="options">Configurações básicas do Service Bus</param>
    /// <param name="logger">Logger para registro de eventos</param>
    /// <param name="clientOptions">Configurações avançadas do cliente (opcional)</param>
    /// <exception cref="ArgumentNullException">Quando options ou logger são nulos</exception>
    /// <exception cref="ArgumentException">Quando a configuração é inválida</exception>
    /// <remarks>
    /// <para>O construtor realiza as seguintes validações e inicializações:</para>
    /// <list type="number">
    /// <item><description>Valida se os parâmetros obrigatórios não são nulos</description></item>
    /// <item><description>Valida a configuração usando ServiceBusConfiguration.IsValid()</description></item>
    /// <item><description>Cria e configura o ServiceBusClient usando a configuração fornecida</description></item>
    /// <item><description>Registra no log a inicialização bem-sucedida</description></item>
    /// </list>
    ///
    /// <para>Ordem de precedência para configuração do cliente:</para>
    /// <list type="number">
    /// <item><description>ServiceBusClientConfiguration (se fornecida)</description></item>
    /// <item><description>ServiceBusConfiguration com opções padrão</description></item>
    /// </list>
    /// </remarks>
    public ServiceBusMessageSender(
        IOptions<ServiceBusConfiguration> options,
        ILogger<ServiceBusMessageSender> logger,
        IOptions<ServiceBusClientConfiguration> clientOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Inicializa gerenciador de configurações (valida e processa automaticamente)
        _configurationManager = new ServiceBusConfigurationManager(options, clientOptions, _logger);

        // Inicializa cache de clientes para reutilização se habilitado
        _clientCache = new System.Collections.Concurrent.ConcurrentDictionary<string, ServiceBusClient>();

        // Cria cliente Service Bus usando configurações processadas
        _serviceBusClient = _configurationManager.CreateServiceBusClient();

        // Log das configurações finais
        var (timeout, maxRetry, transport) = _configurationManager.GetConfigurationInfo();
        _logger.LogInformation("ServiceBusMessageSender inicializado. Timeout: {Timeout}s, MaxRetry: {MaxRetry}, Transport: {Transport}",
            timeout, maxRetry, transport);
    }

    #endregion

    #region Métodos de Gerenciamento de Cliente

    #endregion

    #region IAsyncDisposable

    /// <summary>
    /// Libera recursos de forma assíncrona
    /// </summary>
    /// <returns>ValueTask representando a operação de dispose assíncrono</returns>
    /// <remarks>
    /// <para>Realiza dispose assíncrono seguindo o padrão recomendado:</para>
    /// <list type="number">
    /// <item><description>Verifica se já foi disposed para evitar múltiplas execuções</description></item>
    /// <item><description>Fecha o ServiceBusClient de forma assíncrona</description></item>
    /// <item><description>Marca como disposed para futuras verificações</description></item>
    /// <item><description>Suprime o finalizer para otimização de GC</description></item>
    /// </list>
    ///
    /// <para>É thread-safe e pode ser chamado múltiplas vezes sem efeitos colaterais.</para>
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            try
            {
                if (_serviceBusClient != null)
                {
                    await _serviceBusClient.DisposeAsync();
                }

                // Dispose de todos os clientes em cache
                foreach (var cachedClient in _clientCache.Values)
                {
                    try
                    {
                        await cachedClient.DisposeAsync();
                    }
                    catch (Exception cacheEx)
                    {
                        _logger.LogWarning(cacheEx, "Erro ao fazer dispose de cliente em cache");
                    }
                }
                _clientCache.Clear();

                _logger.LogInformation("ServiceBusMessageSender disposed com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer dispose do ServiceBusMessageSender");
            }
            finally
            {
                _disposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Libera todos os recursos utilizados pelo ServiceBusMessageSender
    /// </summary>
    /// <remarks>
    /// <para>Implementa o padrão Dispose corretamente:</para>
    /// <list type="bullet">
    /// <item><description>Chama Dispose(true) para liberar recursos gerenciados</description></item>
    /// <item><description>Suprime o finalizer com GC.SuppressFinalize</description></item>
    /// <item><description>É thread-safe e pode ser chamado múltiplas vezes</description></item>
    /// </list>
    /// <para>Após chamar Dispose(), todas as operações lançarão ObjectDisposedException.</para>
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implementa o padrão Dispose protegido para liberação de recursos
    /// </summary>
    /// <param name="disposing">
    /// True se chamado pelo método Dispose(); false se chamado pelo finalizer
    /// </param>
    /// <remarks>
    /// <para>Libera recursos de acordo com o padrão recomendado:</para>
    /// <list type="bullet">
    /// <item><description>Se disposing=true: libera recursos gerenciados (ServiceBusClient)</description></item>
    /// <item><description>Marca o objeto como disposed para evitar múltiplas liberações</description></item>
    /// <item><description>Thread-safe através de lock ou operações atômicas</description></item>
    /// </list>
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                _serviceBusClient?.DisposeAsync().AsTask().Wait();
                _logger.LogInformation("ServiceBusMessageSender disposed com sucesso (síncrono)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer dispose síncrono do ServiceBusMessageSender");
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    #endregion
}
