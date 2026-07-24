using System.Security.Cryptography;

namespace Nuuvify.CommonPack.MftMailbox.Utilities;

/// <summary>
/// Utilitário para cálculo de hash SHA-256 de streams de arquivo.
/// </summary>
/// <remarks>
/// Usado internamente pelos clientes MFT para calcular o checksum de integridade
/// após upload ou download. O hash é retornado em hexadecimal minúsculo (sem hífens)
/// e gravado em <see cref="Nuuvify.CommonPack.MftMailbox.Abstraction.Models.TransferItemResult.ChecksumSha256"/>.
/// </remarks>
public static class ChecksumCalculator
{
    /// <summary>
    /// Calcula o hash SHA-256 do conteúdo do stream e retorna em hexadecimal minúsculo.
    /// </summary>
    /// <param name="stream">
    /// Stream cujo conteúdo será hasheado. Se suportar seek, a posição é reiniciada para 0
    /// antes e após o cálculo, de forma que o chamador possa reutilizar o stream.
    /// </param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>String hexadecimal minúscula com 64 caracteres representando o SHA-256 do conteúdo.</returns>
    /// <exception cref="ArgumentNullException">Lançado quando <paramref name="stream"/> é <see langword="null"/>.</exception>
    public static async Task<string> Sha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
