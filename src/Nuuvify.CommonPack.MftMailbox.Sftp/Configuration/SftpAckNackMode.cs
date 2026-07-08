namespace Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;

/// <summary>
/// Define o mecanismo utilizado pelo cliente SFTP para sinalizar ACK ou NACK ao servidor MFT.
/// </summary>
public enum SftpAckNackMode
{
    /// <summary>
    /// Move o arquivo do diretório inbound para o diretório de arquivo de sucesso (ACK)
    /// ou de erro (NACK) usando renomeação SFTP.
    /// Requer que os diretórios <c>ArchiveSuccessDirectory</c> e <c>ArchiveErrorDirectory</c> existam
    /// ou sejam criados automaticamente.
    /// </summary>
    Metadata = 1,

    /// <summary>
    /// Cria um arquivo de marcador no diretório <c>AckMarkerDirectory</c> com o nome
    /// <c>&lt;filename&gt;.ack</c> ou <c>&lt;filename&gt;.nack</c>.
    /// Útil quando o servidor MFT monitora esse diretório para confirmações externas.
    /// </summary>
    MarkerFile = 2
}
