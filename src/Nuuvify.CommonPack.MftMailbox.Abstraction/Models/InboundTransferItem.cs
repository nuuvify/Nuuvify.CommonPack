namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Representa um arquivo recebido do servidor MFT (inbound).
/// </summary>
/// <remarks>
/// Implementa <see cref="IAsyncDisposable"/> para garantir o fechamento do <see cref="Content"/>.
/// O consumidor deve sempre descartar o item após ler o conteúdo:
/// <code>
/// await using var item = await inboundClient.ReceiveSingleAsync(envelope, ct);
/// if (item is not null) { /* processa item.Content */ }
/// </code>
/// </remarks>
public sealed class InboundTransferItem : IAsyncDisposable
{
    /// <summary>Identificador único do item no servidor MFT.</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>Nome do arquivo original no servidor.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Caminho remoto completo do arquivo no servidor MFT.</summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>Tamanho do arquivo em bytes. <see langword="null"/> quando o servidor não informa o tamanho.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Hash SHA-256 do conteúdo em hexadecimal minúsculo, calculado após o download.
    /// <see langword="null"/> quando o checksum não está disponível.
    /// </summary>
    public string? ChecksumSha256 { get; set; }

    /// <summary>
    /// Stream com o conteúdo do arquivo baixado. Leia o stream antes de descartar o item.
    /// O stream pode ou não suportar seek dependendo do protocolo.
    /// </summary>
    public Stream Content { get; set; } = Stream.Null;

    /// <summary>
    /// Metadados adicionais fornecidos pelo servidor ou pela implementação do cliente.
    /// A comparação de chaves é case-insensitive.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync().ConfigureAwait(false);
    }
}
