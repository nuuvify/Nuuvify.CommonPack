using System.Text;

namespace Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

/// <summary>
/// Opções para configuração de mensagens do Azure Service Bus
/// </summary>
/// <remarks>
/// Esta classe contém todas as opções disponíveis para configurar mensagens
/// que serão enviadas através do Azure Service Bus, incluindo metadados,
/// roteamento, tempo de vida e propriedades customizadas.
/// </remarks>
public class ServiceBusMessageOptions
{
    /// <summary>
    /// ID único da mensagem. Se não especificado, será gerado automaticamente pelo Service Bus
    /// </summary>
    /// <remarks>
    /// O MessageId deve ser único dentro do escopo da entidade (queue ou topic).
    /// É usado para detecção de duplicatas quando necessário.
    /// </remarks>
    public string MessageId { get; set; }

    /// <summary>
    /// ID de correlação para rastreamento e associação de mensagens relacionadas
    /// </summary>
    /// <remarks>
    /// Útil para implementar padrões como Request/Response ou para agrupar
    /// mensagens logicamente relacionadas em diferentes transações.
    /// </remarks>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Chave de partição para controle de particionamento e ordenação de mensagens
    /// </summary>
    /// <remarks>
    /// Mensagens com a mesma PartitionKey são garantidas de serem processadas
    /// na mesma partição e em ordem FIFO (First In, First Out).
    /// </remarks>
    public string PartitionKey { get; set; }

    /// <summary>
    /// ID da sessão para mensagens que requerem processamento ordenado
    /// </summary>
    /// <remarks>
    /// Usado para garantir que mensagens relacionadas sejam processadas
    /// sequencialmente pelo mesmo consumidor (session-aware processing).
    /// </remarks>
    public string SessionId { get; set; }

    /// <summary>
    /// Rótulo/Label da mensagem para filtragem e roteamento em subscriptions
    /// </summary>
    /// <remarks>
    /// Usado principalmente com topics e subscriptions para criar filtros
    /// baseados no conteúdo do label. Será mapeado para a propriedade Subject.
    /// </remarks>
    public string Label { get; set; }

    /// <summary>
    /// Endereço de destino para respostas em padrões request/response
    /// </summary>
    /// <remarks>
    /// Especifica a queue ou topic onde respostas devem ser enviadas.
    /// Útil para implementar padrões de comunicação bidirecional.
    /// </remarks>
    public string ReplyTo { get; set; }

    /// <summary>
    /// ID da sessão para onde enviar respostas
    /// </summary>
    /// <remarks>
    /// Usado em conjunto com ReplyTo quando se trabalha com sessões
    /// para garantir que a resposta seja entregue na sessão correta.
    /// </remarks>
    public string ReplyToSessionId { get; set; }

    /// <summary>
    /// Tempo de vida da mensagem (Time To Live - TTL)
    /// </summary>
    /// <remarks>
    /// Define por quanto tempo a mensagem permanecerá válida no broker.
    /// Após este período, a mensagem será movida para Dead Letter Queue ou descartada.
    /// </remarks>
    public TimeSpan TimeToLive { get; set; }

    /// <summary>
    /// Tipo de conteúdo da mensagem (MIME type)
    /// </summary>
    /// <remarks>
    /// Indica o formato do payload da mensagem. Padrão é "application/json".
    /// Útil para consumidores determinarem como desserializar o conteúdo.
    /// </remarks>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Assunto/Subject da mensagem para categorização e filtros
    /// </summary>
    /// <remarks>
    /// Propriedade principal para filtragem em subscriptions de topics.
    /// Tem precedência sobre a propriedade Label quando ambas são definidas.
    /// </remarks>
    public string Subject { get; set; }

    /// <summary>
    /// Destinatário da mensagem para roteamento avançado
    /// </summary>
    /// <remarks>
    /// Campo informativo que pode ser usado para lógica de roteamento
    /// customizada ou para indicar o destinatário final da mensagem.
    /// </remarks>
    public string To { get; set; }

    /// <summary>
    /// Encoding para serialização do conteúdo da mensagem
    /// </summary>
    /// <remarks>
    /// Define como o payload será codificado em bytes. Padrão é UTF-8.
    /// Importante para garantir compatibilidade entre produtores e consumidores.
    /// </remarks>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Propriedades customizadas da aplicação para metadados adicionais
    /// </summary>
    /// <remarks>
    /// Dicionário de chave-valor para metadados customizados que não fazem
    /// parte do payload principal. Útil para filtros e roteamento baseado em propriedades.
    /// Limite total aproximado de 32KB para todas as propriedades.
    /// </remarks>
    public Dictionary<string, object> ApplicationProperties { get; set; } = new();

    /// <summary>
    /// Atraso antes da mensagem ficar disponível para processamento
    /// </summary>
    /// <remarks>
    /// Define um delay relativo ao momento do envio. A mensagem ficará
    /// "invisível" para consumidores até que este tempo se esgote.
    /// Alternativa ao ScheduledEnqueueTime para delays relativos.
    /// </remarks>
    public TimeSpan? ScheduledEnqueueTimeDelay { get; set; }

    /// <summary>
    /// Indica se deve aguardar confirmação de entrega do broker
    /// </summary>
    /// <remarks>
    /// Quando true, o método de envio aguardará confirmação do Service Bus
    /// antes de retornar. Aumenta confiabilidade mas pode impactar performance.
    /// </remarks>
    public bool RequiresDeliveryConfirmation { get; set; } = true;

    /// <summary>
    /// Número máximo de tentativas de entrega antes de mover para Dead Letter Queue
    /// </summary>
    /// <remarks>
    /// Define quantas vezes o Service Bus tentará entregar a mensagem
    /// antes de considera-la como não processável. Configuração específica da entidade.
    /// </remarks>
    public int? MaxDeliveryCount { get; set; }
}
