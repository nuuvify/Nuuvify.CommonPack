namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Resultado agregado de uma operação de envio ou recepção em lote.
/// </summary>
/// <remarks>
/// Retornado por <c>IMftTransferClient.SendBatchAsync</c>. Inspecione <see cref="FailedCount"/>
/// para identificar itens que precisam de reprocessamento manual.
/// </remarks>
public sealed class TransferBatchResult
{
    /// <summary>Instante (UTC) em que o processamento do lote foi iniciado.</summary>
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Instante (UTC) em que o processamento do lote foi concluído (com ou sem erros).</summary>
    public DateTimeOffset FinishedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Resultados individuais de cada item processado no lote.</summary>
    public IList<TransferItemResult> Items { get; set; } = new List<TransferItemResult>();

    /// <summary>Número de itens enviados com sucesso (<see cref="TransferState.Succeeded"/>).</summary>
    public int SucceededCount => Items.Count(x => x.State == TransferState.Succeeded);

    /// <summary>Número de itens que falharam após esgotar os retries (<see cref="TransferState.Failed"/>).</summary>
    public int FailedCount => Items.Count(x => x.State == TransferState.Failed);

    /// <summary>Número de itens ignorados por idempotência (<see cref="TransferState.Skipped"/>).</summary>
    public int SkippedCount => Items.Count(x => x.State == TransferState.Skipped);
}
