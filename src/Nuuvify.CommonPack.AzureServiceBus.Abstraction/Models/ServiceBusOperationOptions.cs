
using Azure.Messaging.ServiceBus;

namespace Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

/// <summary>
/// Opções de cliente para operações específicas do Azure Service Bus
/// </summary>
/// <remarks>
/// Esta classe permite configurar ServiceBusClient e ServiceBusClientOptions
/// para operações específicas, oferecendo flexibilidade para casos onde
/// diferentes operações requerem configurações de cliente distintas.
/// </remarks>
public class ServiceBusOperationOptions
{
    /// <summary>
    /// Cliente Service Bus específico para esta operação
    /// </summary>
    /// <remarks>
    /// Quando fornecido, este cliente será usado ao invés do cliente padrão
    /// configurado no ServiceBusMessageSender. Útil para operações que requerem
    /// configurações específicas de conectividade ou credenciais diferentes.
    /// </remarks>
    public ServiceBusClient CustomClient { get; set; }

    /// <summary>
    /// Opções do cliente Service Bus para esta operação
    /// </summary>
    /// <remarks>
    /// Configurações específicas que serão aplicadas se um novo cliente
    /// precisar ser criado para esta operação. Inclui configurações de
    /// transporte, retry, timeouts e outras propriedades do cliente.
    /// </remarks>
    public ServiceBusClientOptions CustomClientOptions { get; set; }

    /// <summary>
    /// Connection string específica para esta operação
    /// </summary>
    /// <remarks>
    /// Quando fornecida junto com CustomClientOptions, permite criar
    /// um cliente temporário com configurações específicas para esta operação.
    /// Útil para cenários multi-tenant ou multi-environment.
    /// </remarks>
    public string CustomConnectionString { get; set; }

    /// <summary>
    /// Indica se deve criar um cliente temporário para esta operação
    /// </summary>
    /// <remarks>
    /// Quando true e CustomConnectionString/CustomClientOptions são fornecidos,
    /// um cliente temporário será criado e disposto após a operação.
    /// Quando false, o cliente customizado deve ser gerenciado externamente.
    /// </remarks>
    public bool UseTemporaryClient { get; set; } = true;

    /// <summary>
    /// Factory para criação customizada de cliente para esta operação
    /// </summary>
    /// <remarks>
    /// Permite lógica customizada de criação de cliente específica para esta operação.
    /// Tem precedência sobre CustomConnectionString e CustomClientOptions.
    /// </remarks>
    public Func<ServiceBusClient> ClientFactory { get; set; }

    /// <summary>
    /// Valida se as opções são consistentes
    /// </summary>
    /// <returns>True se válidas, false caso contrário</returns>
    /// <remarks>
    /// Verifica se as combinações de propriedades fazem sentido e são válidas
    /// para criação de cliente ou uso de cliente existente.
    /// </remarks>
    public bool IsValid()
    {
        // Se tem CustomClient, não precisa de outras configurações
        if (CustomClient != null)
            return true;

        // Se tem ClientFactory, não precisa de outras configurações
        if (ClientFactory != null)
            return true;

        // Se tem CustomConnectionString, precisa de CustomClientOptions para criar cliente
        if (!string.IsNullOrWhiteSpace(CustomConnectionString))
            return CustomClientOptions != null;

        // Se tem apenas CustomClientOptions sem ConnectionString, não é válido
        if (CustomClientOptions != null)
            return !string.IsNullOrWhiteSpace(CustomConnectionString);

        // Sem configurações customizadas é válido (usa configuração padrão)
        return true;
    }

    /// <summary>
    /// Cria um cliente Service Bus baseado nas configurações desta instância
    /// </summary>
    /// <param name="fallbackConnectionString">Connection string padrão caso não tenha uma customizada</param>
    /// <param name="fallbackOptions">Opções padrão caso não tenha opções customizadas</param>
    /// <returns>ServiceBusClient configurado ou null se deve usar cliente padrão</returns>
    /// <remarks>
    /// Método utilitário para criar cliente baseado nas configurações.
    /// Retorna null quando deve usar o cliente padrão do ServiceBusMessageSender.
    /// </remarks>
    public ServiceBusClient CreateClient(string fallbackConnectionString = null, ServiceBusClientOptions fallbackOptions = null)
    {
        // Se tem cliente customizado, retorna ele
        if (CustomClient != null)
            return CustomClient;

        // Se tem factory customizada, usa ela
        if (ClientFactory != null)
            return ClientFactory();

        // Se tem connection string customizada, cria novo cliente
        if (!string.IsNullOrWhiteSpace(CustomConnectionString))
        {
            var options = CustomClientOptions ?? fallbackOptions ?? new ServiceBusClientOptions();
            return new ServiceBusClient(CustomConnectionString, options);
        }

        // Se tem apenas opções customizadas, usa connection string padrão
        if (CustomClientOptions != null && !string.IsNullOrWhiteSpace(fallbackConnectionString))
        {
            return new ServiceBusClient(fallbackConnectionString, CustomClientOptions);
        }

        // Sem configurações customizadas, usa cliente padrão
        return null;
    }
}
