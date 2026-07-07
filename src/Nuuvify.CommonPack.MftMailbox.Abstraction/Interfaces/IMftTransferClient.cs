using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

/// <summary>
/// Contrato para envio (outbound/upload) de arquivos via MFT.
/// </summary>
/// <remarks>
/// Encapsula toda a lógica de idempotência, checksum e resiliência para o envio de arquivos.
/// O consumidor monta um <see cref="TransferEnvelope"/> com os itens a enviar e chama
/// <see cref="SendSingleAsync"/> ou <see cref="SendBatchAsync"/> de acordo com o volume.
/// Arquivos já processados (detectados pelo store de idempotência) têm estado <see cref="TransferState.Skipped"/>.
/// </remarks>
public interface IMftTransferClient
{
    /// <summary>
    /// Envia exatamente um arquivo para o servidor MFT.
    /// </summary>
    /// <param name="envelope">Envelope com exatamente um <see cref="TransferItem"/> em <see cref="TransferEnvelope.Items"/>.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <see cref="TransferItemResult"/> com o resultado do envio. O estado é <see cref="TransferState.Succeeded"/>,
    /// <see cref="TransferState.Failed"/> ou <see cref="TransferState.Skipped"/> (item já enviado anteriormente).
    /// </returns>
    /// <exception cref="ArgumentException">Lançado quando o envelope não contém exatamente um item.</exception>
    Task<TransferItemResult> SendSingleAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia múltiplos arquivos para o servidor MFT em uma única operação.
    /// </summary>
    /// <param name="envelope">Envelope com um ou mais <see cref="TransferItem"/> em <see cref="TransferEnvelope.Items"/>.
    /// O total não pode exceder <c>MftMailboxOptions.MaxBatchSize</c> (padrão: 500).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <see cref="TransferBatchResult"/> com o resultado agregado de cada item,
    /// incluindo contagens de sucesso, falha e itens ignorados por idempotência.
    /// </returns>
    /// <exception cref="InvalidOperationException">Lançado quando o número de itens excede o limite configurado.</exception>
    Task<TransferBatchResult> SendBatchAsync(TransferEnvelope envelope, CancellationToken cancellationToken = default);
}
