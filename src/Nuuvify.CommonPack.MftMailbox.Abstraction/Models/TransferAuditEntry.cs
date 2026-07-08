namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Entrada de auditoria gerada após cada operação de transferência MFT.
/// </summary>
/// <remarks>
/// Passada para <c>ITransferAuditSink.WriteAsync</c> ao final de envios, recepções e ACK/NACK.
/// Implemente <c>ITransferAuditSink</c> para persistir essas entradas em banco de dados, log ou telemetria.
/// </remarks>
public sealed class TransferAuditEntry
{
    /// <summary>Instante (UTC) em que a operação ocorreu. Preenchido automaticamente com <see cref="DateTimeOffset.UtcNow"/>.</summary>
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Chave que identifica a integração no sistema MFT.</summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>Identificador de correlação da operação, propagado do <see cref="TransferEnvelope.CorrelationId"/>.</summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>Identificador único do item auditado.</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>Nome do arquivo auditado.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Protocolo utilizado na operação (<see cref="MftProtocol.Sftp"/> ou <see cref="MftProtocol.Https"/>).</summary>
    public MftProtocol Protocol { get; set; }

    /// <summary>Estado resultante da operação auditada.</summary>
    public TransferState State { get; set; }

    /// <summary>Mensagem descritiva do resultado, incluindo detalhes de erro quando aplicável.</summary>
    public string? Message { get; set; }

    /// <summary>
    /// Metadados adicionais propagados do envelope ou definidos pela implementação do cliente.
    /// A comparação de chaves é case-insensitive.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
