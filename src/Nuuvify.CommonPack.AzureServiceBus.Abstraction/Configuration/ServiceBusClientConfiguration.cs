using Azure.Messaging.ServiceBus;

namespace Nuuvify.CommonPack.AzureServiceBus.Abstraction.Configuration;

/// <summary>
/// Configuração estendida para Azure Service Bus Client com opções avançadas
/// </summary>
/// <remarks>
/// Esta classe estende ServiceBusConfiguration permitindo configuração detalhada
/// do ServiceBusClient e ServiceBusClientOptions, oferecendo controle granular
/// sobre conectividade, transporte, retry policies e outras configurações avançadas.
/// </remarks>
public class ServiceBusClientConfiguration : ServiceBusConfiguration
{
    /// <summary>
    /// Configurações específicas do ServiceBusClient
    /// </summary>
    /// <remarks>
    /// Permite personalizar comportamentos como tipo de transporte, timeouts,
    /// retry policies e outras configurações do cliente Service Bus.
    /// Se não especificado, usa configurações padrão otimizadas.
    /// </remarks>
    public ServiceBusClientOptions ClientOptions { get; set; }

    /// <summary>
    /// Instância pré-configurada do ServiceBusClient para reutilização
    /// </summary>
    /// <remarks>
    /// Quando fornecida, esta instância será usada ao invés de criar um novo cliente.
    /// Útil para cenários onde o cliente é compartilhado entre múltiplos serviços
    /// ou quando configurações específicas de conectividade são necessárias.
    /// ATENÇÃO: O ciclo de vida do client deve ser gerenciado externamente.
    /// </remarks>
    public ServiceBusClient PreConfiguredClient { get; set; }

    /// <summary>
    /// Factory para criação customizada de ServiceBusClient
    /// </summary>
    /// <remarks>
    /// Delegate que permite criação totalmente customizada do cliente Service Bus.
    /// Recebe a connection string e as opções de cliente como parâmetros,
    /// permitindo lógica de criação complexa ou integração com outros sistemas.
    /// </remarks>
    public Func<string, ServiceBusClientOptions, ServiceBusClient> ClientFactory { get; set; }

    /// <summary>
    /// Indica se deve reutilizar conexões existentes (padrão: true)
    /// </summary>
    /// <remarks>
    /// Quando true, otimiza uso de recursos reutilizando conexões TCP.
    /// Quando false, cria nova conexão para cada operação (útil para debug).
    /// </remarks>
    public bool ReuseConnections { get; set; } = true;

    /// <summary>
    /// Configurações específicas de transporte
    /// </summary>
    /// <remarks>
    /// Define o tipo de transporte usado para comunicação com Service Bus:
    /// - AmqpTcp: AMQP sobre TCP (padrão, melhor performance)
    /// - AmqpWebSockets: AMQP sobre WebSockets (para firewalls restritivos)
    /// </remarks>
    public ServiceBusTransportType TransportType { get; set; } = ServiceBusTransportType.AmqpWebSockets;

    /// <summary>
    /// Configurações customizadas de retry policy
    /// </summary>
    /// <remarks>
    /// Permite configurar estratégia de retry mais granular que as configurações básicas.
    /// Inclui configurações como delay entre tentativas, jitter, e tipos de exceções
    /// que devem ser retriadas.
    /// </remarks>
    public ServiceBusRetryOptions RetryOptions { get; set; }

    /// <summary>
    /// Proxy para conexões HTTP (usado com WebSockets)
    /// </summary>
    /// <remarks>
    /// Configurações de proxy web quando TransportType é AmqpWebSockets.
    /// Necessário em ambientes corporativos com proxy obrigatório.
    /// </remarks>
    public System.Net.WebProxy WebProxy { get; set; }

    /// <summary>
    /// Cria ServiceBusClientOptions baseado nas configurações atuais
    /// </summary>
    /// <returns>ServiceBusClientOptions configurado</returns>
    /// <remarks>
    /// Método utilitário que gera ServiceBusClientOptions baseado nas propriedades
    /// definidas nesta configuração. Usado internamente pelo ServiceBusMessageSender
    /// quando ClientOptions não é fornecido explicitamente.
    /// </remarks>
    public ServiceBusClientOptions CreateClientOptions()
    {
        var options = ClientOptions ?? new ServiceBusClientOptions();

        // Aplica configurações desta classe se não foram definidas nas ClientOptions
        if (options.TransportType == default)
        {
            options.TransportType = TransportType;
        }

        if (RetryOptions != null && options.RetryOptions == null)
        {
            options.RetryOptions = RetryOptions;
        }

        if (WebProxy != null && options.WebProxy == null)
        {
            options.WebProxy = WebProxy;
        }

        return options;
    }

    /// <summary>
    /// Cria ServiceBusClient usando as configurações atuais
    /// </summary>
    /// <returns>ServiceBusClient configurado</returns>
    /// <remarks>
    /// Método que cria uma nova instância de ServiceBusClient baseada nas configurações.
    /// Ordem de precedência:
    /// 1. PreConfiguredClient (se fornecido)
    /// 2. ClientFactory (se fornecido)
    /// 3. Criação padrão com connection string e opções
    /// </remarks>
    public ServiceBusClient CreateClient()
    {
        // Se há um cliente pré-configurado, usa ele
        if (PreConfiguredClient != null)
        {
            return PreConfiguredClient;
        }

        var clientOptions = CreateClientOptions();

        // Se há uma factory customizada, usa ela
        if (ClientFactory != null)
        {
            return ClientFactory(ConnectionString, clientOptions);
        }

        // Criação padrão
        return new ServiceBusClient(ConnectionString, clientOptions);
    }

    /// <summary>
    /// Valida a configuração estendida
    /// </summary>
    /// <returns>True se válida, false caso contrário</returns>
    public new bool IsValid()
    {
        var baseValid = base.IsValid();

        if (!baseValid)
            return false;

        // Se há PreConfiguredClient, não precisa de ConnectionString
        if (PreConfiguredClient != null)
            return true;

        // Se há ClientFactory, precisa da ConnectionString
        if (ClientFactory != null)
            return !string.IsNullOrWhiteSpace(ConnectionString);

        return true;
    }
}
