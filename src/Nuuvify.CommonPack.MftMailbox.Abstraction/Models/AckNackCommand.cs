namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Comando de confirmação (ACK) ou rejeição (NACK) de um arquivo recebido via MFT.
/// </summary>
/// <remarks>
/// Montado pelo consumidor após processar o conteúdo de um <see cref="InboundTransferItem"/>
/// e passado para <c>IAckNackClient.AckOrNackAsync</c>. O servidor MFT usa a decisão
/// para mover o arquivo para o diretório de sucesso ou de erro.
/// </remarks>
public sealed class AckNackCommand
{
    /// <summary>
    /// Chave que identifica a integração no sistema MFT (ex.: código do sistema parceiro).
    /// Deve coincidir com o valor usado no <see cref="TransferEnvelope.IntegrationKey"/> correspondente.
    /// </summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de correlação que associa o comando à operação de recepção original.
    /// Utilize o mesmo <see cref="TransferEnvelope.CorrelationId"/> do envelope de entrada.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único do item dentro da transferência. Obrigatório.
    /// Corresponde ao <see cref="InboundTransferItem.ItemId"/> recebido.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Nome do arquivo no servidor MFT. Quando vazio, o cliente usa <see cref="ItemId"/> como fallback.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Protocolo utilizado na recepção original. Deve coincidir com o protocolo do cliente que recebeu o arquivo.</summary>
    public MftProtocol Protocol { get; set; }

    /// <summary>Decisão do consumidor: <see cref="AckNackType.Ack"/> para sucesso ou <see cref="AckNackType.Nack"/> para rejeição.</summary>
    public AckNackType Decision { get; set; }

    /// <summary>
    /// Motivo opcional da rejeição. Relevante apenas quando <see cref="Decision"/> é <see cref="AckNackType.Nack"/>.
    /// Gravado no arquivo de marcador (modo <c>SftpAckNackMode.MarkerFile</c>) ou em log de auditoria.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Metadados adicionais a serem propagados para o sink de auditoria.
    /// A comparação de chaves é case-insensitive.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
