namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Representa um arquivo individual a ser enviado para o servidor MFT.
/// </summary>
/// <remarks>
/// O conteúdo do arquivo é fornecido via <see cref="ContentFactory"/>, um delegate assíncrono
/// que retorna um <see cref="Stream"/>. Isso permite que o stream seja aberto sob demanda,
/// evitando manter arquivos grandes em memória durante a construção do envelope.
/// </remarks>
public sealed class TransferItem
{
    /// <summary>
    /// Identificador único do item, gerado automaticamente como GUID sem hífens.
    /// Pode ser substituído por um ID externo para garantir idempotência entre execuções.
    /// </summary>
    public string ItemId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Nome do arquivo que será criado no servidor MFT.
    /// Deve incluir a extensão e não pode conter separadores de caminho; use <see cref="RemotePath"/> para o diretório.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Diretório remoto de destino no servidor MFT. Quando vazio, o cliente usa o diretório
    /// padrão configurado em <c>SftpMftMailboxOptions.OutboundDirectory</c> ou <c>HttpMftMailboxOptions.UploadPath</c>.
    /// </summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>
    /// Tamanho estimado do arquivo em bytes. Informativo; não valida o conteúdo do stream.
    /// </summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Metadados adicionais associados ao item, propagados para auditoria e logs.
    /// A comparação de chaves é case-insensitive.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Fábrica que produz o <see cref="Stream"/> com o conteúdo do arquivo.
    /// Chamada sob demanda no momento do envio, permitindo leitura direta do disco ou de memória.
    /// Obrigatório para operações de envio; ignorado em operações de recepção.
    /// </summary>
    public Func<CancellationToken, Task<Stream>>? ContentFactory { get; set; }
}
