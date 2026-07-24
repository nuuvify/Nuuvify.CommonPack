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
    /// <exception cref="ArgumentNullException">Lançado quando <paramref name="envelope"/> ou <paramref name="item"/> são <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Lançado quando campos obrigatórios da chave estão ausentes.</exception>
    public static string Build(TransferEnvelope envelope, TransferItem item)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(envelope.IntegrationKey))
        {
            throw new ArgumentException("IntegrationKey is required.", nameof(envelope));
        }

        if (string.IsNullOrWhiteSpace(envelope.CorrelationId))
        {
            throw new ArgumentException("CorrelationId is required.", nameof(envelope));
        }

        if (string.IsNullOrWhiteSpace(item.ItemId))
        {
            throw new ArgumentException("ItemId is required.", nameof(item));
        }

        if (string.IsNullOrWhiteSpace(item.FileName))
        {
            throw new ArgumentException("FileName is required.", nameof(item));
        }

        return string.Join(
            "|",
            envelope.IntegrationKey.Trim(),
            envelope.CorrelationId.Trim(),
            item.ItemId.Trim(),
            item.FileName.Trim()).ToLowerInvariant();
    }
}
