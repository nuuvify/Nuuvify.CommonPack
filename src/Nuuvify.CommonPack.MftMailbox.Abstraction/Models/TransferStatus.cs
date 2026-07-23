namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Snapshot do estado atual de uma transferência MFT consultada via <c>IMftStatusClient</c>.
/// </summary>
/// <remarks>
/// A implementação SFTP mantém status em memória durante a vida do cliente; a HTTP consulta
/// o endpoint remoto. Use esta classe para polling de confirmação e diagnóstico de falhas.
/// </remarks>
public sealed class TransferStatus
{
    /// <summary>Chave de integração associada à transferência.</summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>Identificador do item cujo status é descrito.</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>Chave de idempotência composta que identificou univocamente esta transferência.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Estado atual da transferência.</summary>
    public TransferState State { get; set; }

    /// <summary>Status externo retornado pelo servidor MFT (ex.: código de status da API parceira).</summary>
    public string? ExternalStatus { get; set; }

    /// <summary>Hash SHA-256 do arquivo confirmado pelo servidor, quando disponível.</summary>
    public string? ChecksumSha256 { get; set; }

    /// <summary>Tamanho confirmado pelo servidor em bytes.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>Número de tentativas realizadas até o estado atual.</summary>
    public int Attempts { get; set; }

    /// <summary>Instante (UTC) da última atualização do status.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Descrição do erro quando <see cref="State"/> é <see cref="TransferState.Failed"/>.</summary>
    public string? Error { get; set; }
}
