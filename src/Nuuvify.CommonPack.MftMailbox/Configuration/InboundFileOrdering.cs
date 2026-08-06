namespace Nuuvify.CommonPack.MftMailbox.Configuration;

/// <summary>
/// Define a ordenação aplicada aos itens inbound antes do processamento.
/// </summary>
public enum InboundFileOrdering
{
    /// <summary>
    /// Mantém a ordem original retornada pela origem (servidor SFTP/HTTP).
    /// </summary>
    None = 0,

    /// <summary>
    /// Ordena pelo nome do arquivo em ordem crescente (A-Z).
    /// </summary>
    FileNameAscending = 1,

    /// <summary>
    /// Ordena pelo nome do arquivo em ordem decrescente (Z-A).
    /// </summary>
    FileNameDescending = 2
}
