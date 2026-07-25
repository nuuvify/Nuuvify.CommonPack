namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Unidade de entrega que agrupa um ou mais <see cref="TransferItem"/> para uma operação MFT.
/// </summary>
/// <remarks>
/// O envelope é o objeto principal passado para <c>IMftTransferClient</c>, <c>IMftInboundClient</c>
/// e <c>IAckNackClient</c>. Defina <see cref="IntegrationKey"/> de forma consistente; ela é usada
/// na composição da chave de idempotência e nos registros de auditoria.
/// </remarks>
public sealed class TransferEnvelope
{
    /// <summary>
    /// Chave que identifica a integração no servidor MFT (ex.: código do sistema ou canalização).
    /// Obrigatória e não pode ser vazia.
    /// </summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de correlação gerado automaticamente como GUID sem hífens.
    /// Pode ser substituído para propagar um trace ID existente (ex.: Activity.TraceId).
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Protocolo de transferência a ser utilizado para todos os itens do envelope.</summary>
    public MftProtocol Protocol { get; set; }

    /// <summary>
    /// Lista de itens a serem transferidos. Para operações de envio, cada item deve ter
    /// <see cref="TransferItem.ContentFactory"/> configurado. Para recepção, pode ficar
    /// vazia para que o cliente liste os arquivos disponíveis automaticamente.
    /// </summary>
    public IList<TransferItem> Items { get; set; } = new List<TransferItem>();

    /// <summary>
    /// Metadados adicionais propagados para auditoria e que podem ser usados pela implementação
    /// para enriquecer headers ou logs. A comparação de chaves é case-insensitive.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
