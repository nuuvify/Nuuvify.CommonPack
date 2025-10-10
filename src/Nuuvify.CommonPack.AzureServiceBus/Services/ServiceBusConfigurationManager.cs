using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Configuration;

namespace Nuuvify.CommonPack.AzureServiceBus.Services;

/// <summary>
/// Gerenciador responsável por processar e validar configurações do Azure Service Bus
/// </summary>
/// <remarks>
/// Esta classe centraliza a lógica de processamento de configurações, aplicando
/// fallbacks entre ServiceBusConfiguration e ServiceBusClientConfiguration,
/// validando configurações e criando clientes Service Bus apropriados.
/// </remarks>
internal sealed class ServiceBusConfigurationManager
{
    private readonly ServiceBusConfiguration _baseConfiguration;
    private readonly ServiceBusClientConfiguration _clientConfiguration;
    private readonly ILogger _logger;

    /// <summary>
    /// Configuração base processada (sempre válida após construção)
    /// </summary>
    public ServiceBusConfiguration BaseConfiguration { get; private set; }

    /// <summary>
    /// Configuração de cliente processada (pode ser null)
    /// </summary>
    public ServiceBusClientConfiguration ClientConfiguration { get; private set; }

    /// <summary>
    /// Indica se há configuração de cliente avançada disponível
    /// </summary>
    public bool HasClientConfiguration => ClientConfiguration != null;

    /// <summary>
    /// Inicializa o gerenciador de configurações com validação e processamento automático
    /// </summary>
    /// <param name="baseOptions">Configuração básica do Service Bus</param>
    /// <param name="clientOptions">Configuração avançada do cliente (opcional)</param>
    /// <param name="logger">Logger para registro de eventos</param>
    /// <exception cref="ArgumentNullException">Quando parâmetros obrigatórios são nulos</exception>
    /// <exception cref="ArgumentException">Quando configuração é inválida</exception>
    public ServiceBusConfigurationManager(
        IOptions<ServiceBusConfiguration> baseOptions,
        IOptions<ServiceBusClientConfiguration> clientOptions,
        ILogger logger)
    {
        _baseConfiguration = baseOptions?.Value ?? throw new ArgumentNullException(nameof(baseOptions));
        _clientConfiguration = clientOptions?.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateBaseConfiguration();
        ProcessConfigurations();
    }

    /// <summary>
    /// Cria um ServiceBusClient baseado nas configurações processadas
    /// </summary>
    /// <returns>ServiceBusClient configurado</returns>
    /// <remarks>
    /// Utiliza a configuração mais específica disponível:
    /// 1. ServiceBusClientConfiguration (se disponível)
    /// 2. ServiceBusConfiguration com opções padrão
    /// </remarks>
    public ServiceBusClient CreateServiceBusClient()
    {
        if (HasClientConfiguration && !string.IsNullOrWhiteSpace(ClientConfiguration!.ConnectionString))
        {
            var client = ClientConfiguration.CreateClient();
            _logger.LogInformation("ServiceBusClient criado com configuração avançada. Tipo: {ClientType}",
                ClientConfiguration.PreConfiguredClient != null ? "PreConfigurado" : "Personalizado");
            return client;
        }

        var defaultOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        var basicClient = new ServiceBusClient(BaseConfiguration.ConnectionString, defaultOptions);
        _logger.LogInformation("ServiceBusClient criado com configuração básica");
        return basicClient;
    }

    /// <summary>
    /// Obtém o tipo de transporte configurado
    /// </summary>
    /// <returns>Tipo de transporte do Service Bus</returns>
    public ServiceBusTransportType GetTransportType()
    {
        return ClientConfiguration?.TransportType ?? ServiceBusTransportType.AmqpWebSockets;
    }

    /// <summary>
    /// Obtém informações de configuração para logging
    /// </summary>
    /// <returns>Tupla com timeout, retry attempts e transport type</returns>
    public (int Timeout, int MaxRetry, ServiceBusTransportType Transport) GetConfigurationInfo()
    {
        var effectiveConfig = ClientConfiguration ?? BaseConfiguration;
        return (
            effectiveConfig.OperationTimeoutSeconds,
            effectiveConfig.MaxRetryAttempts,
            GetTransportType()
        );
    }

    /// <summary>
    /// Valida a configuração base
    /// </summary>
    /// <exception cref="ArgumentException">Quando configuração é inválida</exception>
    private void ValidateBaseConfiguration()
    {
        if (!_baseConfiguration.IsValid())
        {
            var errors = _baseConfiguration.ValidateConfiguration();
            var errorMessage = string.Join(", ", errors);
            _logger.LogError("Configuração inválida: {Errors}", errorMessage);
            throw new ArgumentException($"Configuração inválida: {errorMessage}", nameof(ServiceBusConfiguration));
        }
    }

    /// <summary>
    /// Processa configurações aplicando fallbacks necessários
    /// </summary>
    private void ProcessConfigurations()
    {
        // Sempre usa a configuração base processada
        BaseConfiguration = _baseConfiguration;

        // Processa configuração de cliente se disponível
        if (_clientConfiguration != null)
        {
            ApplyConfigurationFallbacks();
            ClientConfiguration = _clientConfiguration;
            _logger.LogDebug("Configuração de cliente processada com fallbacks aplicados");
        }
        else
        {
            _logger.LogDebug("Usando apenas configuração básica");
        }
    }

    /// <summary>
    /// Aplica fallbacks da configuração base para a configuração de cliente
    /// </summary>
    private void ApplyConfigurationFallbacks()
    {
        if (_clientConfiguration == null) return;

        // Fallback para propriedades de string
        ApplyStringFallbacks();

        // Fallback para propriedades numéricas
        ApplyNumericFallbacks();

        _logger.LogDebug("Fallbacks de configuração aplicados com sucesso");
    }

    /// <summary>
    /// Aplica fallbacks para propriedades de string
    /// </summary>
    private void ApplyStringFallbacks()
    {
        if (_clientConfiguration == null) return;

        _clientConfiguration.ConnectionString = GetEffectiveStringValue(
            _clientConfiguration.ConnectionString, _baseConfiguration.ConnectionString);

        _clientConfiguration.QueueName = GetEffectiveStringValue(
            _clientConfiguration.QueueName, _baseConfiguration.QueueName);

        _clientConfiguration.TopicName = GetEffectiveStringValue(
            _clientConfiguration.TopicName, _baseConfiguration.TopicName);

        _clientConfiguration.TopicSubscription = GetEffectiveStringValue(
            _clientConfiguration.TopicSubscription, _baseConfiguration.TopicSubscription);
    }

    /// <summary>
    /// Aplica fallbacks para propriedades numéricas
    /// </summary>
    private void ApplyNumericFallbacks()
    {
        if (_clientConfiguration!.OperationTimeoutSeconds == 0)
            _clientConfiguration.OperationTimeoutSeconds = _baseConfiguration.OperationTimeoutSeconds;

        if (_clientConfiguration.MaxRetryAttempts == 0)
            _clientConfiguration.MaxRetryAttempts = _baseConfiguration.MaxRetryAttempts;

        if (_clientConfiguration.RetryDelaySeconds == 0)
            _clientConfiguration.RetryDelaySeconds = _baseConfiguration.RetryDelaySeconds;

        if (_clientConfiguration.MaxBatchSize == 0)
            _clientConfiguration.MaxBatchSize = _baseConfiguration.MaxBatchSize;

        if (_clientConfiguration.DefaultMessageTtlMinutes == 0)
            _clientConfiguration.DefaultMessageTtlMinutes = _baseConfiguration.DefaultMessageTtlMinutes;
    }

    /// <summary>
    /// Obtém o valor efetivo para propriedades de string
    /// </summary>
    /// <param name="clientValue">Valor da configuração de cliente</param>
    /// <param name="baseValue">Valor da configuração base</param>
    /// <returns>Valor efetivo a ser usado</returns>
    private static string GetEffectiveStringValue(string clientValue, string baseValue)
    {
        return string.IsNullOrWhiteSpace(clientValue) ? baseValue : clientValue;
    }

    /// <summary>
    /// Método estático para validar configuração (compatibilidade com código existente)
    /// </summary>
    /// <param name="configuration">Configuração a ser validada</param>
    /// <exception cref="ArgumentException">Quando configuração é inválida</exception>
    public static void ValidateConfiguration(ServiceBusConfiguration configuration)
    {
        if (!configuration.IsValid())
        {
            var errors = configuration.ValidateConfiguration();
            var errorMessage = string.Join(", ", errors);
            throw new ArgumentException($"Configuração inválida: {errorMessage}", nameof(configuration));
        }
    }
}
