using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

namespace Nuuvify.CommonPack.MftMailbox.Utilities;

/// <summary>
/// Constrói chaves de idempotência compostas para identificação única de transferências MFT.
/// </summary>
/// <remarks>
/// A chave é formada pela concatenação (case-insensitive) de:
/// <c>integrationKey|correlationId|itemId|fileName</c>.
/// Garante que o mesmo arquivo não seja reenviado em cenários de retry ou reinicialização.
/// </remarks>
public static class IdempotencyKeyBuilder
{
    /// <summary>
    /// Gera a chave de idempotência a partir do envelope e do item.
    /// </summary>
    /// <param name="envelope">Envelope que contém <see cref="TransferEnvelope.IntegrationKey"/> e <see cref="TransferEnvelope.CorrelationId"/>.</param>
    /// <param name="item">Item que contém <see cref="TransferItem.ItemId"/> e <see cref="TransferItem.FileName"/>.</param>
    /// <returns>String em minúsculo com os quatro campos separados por <c>|</c>.</returns>
    public static string Build(TransferEnvelope envelope, TransferItem item)
    {
        return string.Join(
            "|",
            envelope.IntegrationKey.Trim(),
            envelope.CorrelationId.Trim(),
            item.ItemId.Trim(),
            item.FileName.Trim()).ToLowerInvariant();
    }
}
