using System.Collections.ObjectModel;

/// <summary>
/// Configurações para conexão e operações com o Azure Service Bus
/// </summary>
/// <remarks>
/// Esta classe contém todas as configurações necessárias para estabelecer
/// conexão com o Azure Service Bus e definir comportamentos padrão para
/// operações de envio, retry, timeouts e outras configurações globais.
/// </remarks>
public class ServiceBusConfiguration
{
    /// <summary>
    /// String de conexão do Azure Service Bus
    /// </summary>
    /// <remarks>
    /// Connection string completa obtida do portal Azure ou configurada via
    /// Azure Key Vault. Contém as credenciais e endpoint necessários para conectar.
    /// Formato: Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...
    /// </remarks>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Nome da fila padrão para operações que não especificam uma fila
    /// </summary>
    /// <remarks>
    /// Fila que será usada quando métodos de envio não especificarem
    /// explicitamente o nome da fila de destino.
    /// </remarks>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Nome do tópico padrão para operações que não especificam um tópico
    /// </summary>
    /// <remarks>
    /// Tópico que será usado quando métodos de envio não especificarem
    /// explicitamente o nome do tópico de destino.
    /// </remarks>
    public string TopicName { get; set; } = string.Empty;

    /// <summary>
    /// Nome da subscription padrão para operações de recebimento de mensagens de tópicos
    /// </summary>
    /// <remarks>
    /// Subscription que será usada quando métodos de recebimento não especificarem
    /// explicitamente o nome da subscription. No Azure Service Bus, as subscriptions
    /// são necessárias para receber mensagens de tópicos, funcionando como filtros
    /// que determinam quais mensagens cada consumidor receberá.
    /// </remarks>
    public string TopicSubscription { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para operações individuais em segundos (padrão: 30)
    /// </summary>
    /// <remarks>
    /// Define o tempo limite para operações síncronas com o Service Bus,
    /// incluindo envio de mensagens, criação de senders, etc.
    /// Valor muito baixo pode causar timeouts desnecessários em redes lentas.
    /// </remarks>
    public int OperationTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número máximo de tentativas para retry em caso de falha temporária (padrão: 3)
    /// </summary>
    /// <remarks>
    /// Quantas vezes uma operação será tentada novamente antes de falhar definitivamente.
    /// Aplica-se apenas a falhas temporárias (timeout, service busy, etc.).
    /// Falhas permanentes (autorização, entidade não encontrada) não são retriadas.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay base entre tentativas de retry em segundos (padrão: 5)
    /// </summary>
    /// <remarks>
    /// Tempo base de espera entre tentativas. O delay real cresce exponencialmente:
    /// 1ª tentativa: RetryDelaySeconds * 1
    /// 2ª tentativa: RetryDelaySeconds * 2
    /// 3ª tentativa: RetryDelaySeconds * 3, etc.
    /// </remarks>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Tamanho máximo do batch para envios em lote (padrão: 100)
    /// </summary>
    /// <remarks>
    /// Número máximo de mensagens que serão agrupadas em um único batch
    /// para envio. Batches maiores são mais eficientes mas têm limite de
    /// tamanho total (~1MB por batch no Service Bus).
    /// </remarks>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Indica se deve habilitar sessions para mensagens ordenadas (padrão: false)
    /// </summary>
    /// <remarks>
    /// Quando habilitado, permite uso de sessões para garantir processamento
    /// ordenado de mensagens relacionadas. Requer que queues/topics tenham
    /// sessions habilitadas na configuração da entidade.
    /// </remarks>
    public bool EnableSessions { get; set; } = false;

    /// <summary>
    /// Indica se deve usar particionamento para alta throughput (padrão: false)
    /// </summary>
    /// <remarks>
    /// Particionamento melhora throughput e disponibilidade distribuindo
    /// mensagens entre múltiplas partições. Deve corresponder à configuração
    /// da entidade no Azure Service Bus.
    /// </remarks>
    public bool EnablePartitioning { get; set; } = false;

    /// <summary>
    /// TTL (Time To Live) padrão para mensagens em minutos (padrão: 60)
    /// </summary>
    /// <remarks>
    /// Tempo de vida padrão aplicado a mensagens que não especificam TTL
    /// explícito. Após este período, mensagens não processadas são movidas
    /// para Dead Letter Queue ou descartadas.
    /// </remarks>
    public int DefaultMessageTtlMinutes { get; set; } = 60;

    /// <summary>
    /// Valida se a configuração está completa e correta
    /// </summary>
    /// <returns>True se a configuração está válida, false caso contrário</returns>
    /// <remarks>
    /// Verifica se os campos obrigatórios estão preenchidos e se os valores
    /// estão dentro de ranges aceitáveis para operação com Service Bus.
    /// </remarks>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ConnectionString) &&
               OperationTimeoutSeconds > 0 &&
               MaxRetryAttempts >= 0 &&
               RetryDelaySeconds >= 0 &&
               MaxBatchSize > 0 &&
               DefaultMessageTtlMinutes > 0;
    }

    /// <summary>
    /// Valida a configuração e retorna lista de erros encontrados
    /// </summary>
    /// <returns>Lista de mensagens de erro, vazia se configuração válida</returns>
    public ReadOnlyCollection<string> ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("ConnectionString é obrigatória");

        if (OperationTimeoutSeconds <= 0)
            errors.Add("OperationTimeoutSeconds deve ser maior que zero");

        if (MaxRetryAttempts < 0)
            errors.Add("MaxRetryAttempts não pode ser negativo");

        if (RetryDelaySeconds < 0)
            errors.Add("RetryDelaySeconds não pode ser negativo");

        if (MaxBatchSize <= 0)
            errors.Add("MaxBatchSize deve ser maior que zero");

        if (DefaultMessageTtlMinutes <= 0)
            errors.Add("DefaultMessageTtlMinutes deve ser maior que zero");

        return errors.AsReadOnly();
    }
}
