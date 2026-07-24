namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Protocolo de transferência de arquivos MFT suportado pela biblioteca.
/// </summary>
public enum MftProtocol
{
    /// <summary>Não especifica protocolo de transferência.</summary>
    None = 0,

    /// <summary>Transferência via SSH File Transfer Protocol (SFTP). Utiliza SSH na porta 22 por padrão.</summary>
    Sftp = 1,

    /// <summary>Transferência via HTTPS com API REST. Suporta mutual TLS e Bearer token.</summary>
    Https = 2
}
