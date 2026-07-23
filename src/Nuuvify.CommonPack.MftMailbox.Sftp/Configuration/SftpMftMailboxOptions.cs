namespace Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;

/// <summary>
/// Opções de configuração do cliente SFTP para transferência MFT.
/// </summary>
/// <remarks>
/// Configurado via <c>services.AddMftMailboxSftp(sftp =&gt; { ... })</c> ou pelo arquivo
/// de configuração. A autenticação suporta senha e chave privada (com passphrase opcional).
/// Não informe ambos; a implementação tenta chave privada quando <see cref="PrivateKeyPath"/> está definido.
/// </remarks>
public sealed class SftpMftMailboxOptions
{
    /// <summary>Endereço do servidor SFTP (hostname ou IP). Obrigatório.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Porta do servidor SFTP. Padrão: 22.</summary>
    public int Port { get; set; } = 22;

    /// <summary>Nome de usuário para autenticação SSH. Obrigatório.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha para autenticação por senha. Use <see cref="PrivateKeyPath"/> para autenticação por chave.
    /// Não armazene senhas em texto claro; prefira variáveis de ambiente ou Azure Key Vault.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Caminho local para o arquivo de chave privada SSH (formato OpenSSH ou PEM).
    /// Quando informado, a autenticação por chave tem prioridade sobre <see cref="Password"/>.
    /// </summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>Passphrase da chave privada, quando protegida por senha.</summary>
    public string? PrivateKeyPassphrase { get; set; }

    /// <summary>
    /// Fingerprint SHA-256 ou MD5 da chave pública do servidor para validação de host.
    /// Quando <see langword="null"/>, a validação do host é ignorada (não recomendado em produção).
    /// </summary>
    public string? HostKeyFingerprint { get; set; }

    /// <summary>Diretório remoto de destino para arquivos enviados (outbound/upload). Padrão: <c>/outbound</c>.</summary>
    public string OutboundDirectory { get; set; } = "/outbound";

    /// <summary>Diretório remoto de origem para arquivos recebidos (inbound/download). Padrão: <c>/inbound</c>.</summary>
    public string InboundDirectory { get; set; } = "/inbound";

    /// <summary>
    /// Diretório de arquivo para arquivos confirmados com sucesso (modo <see cref="SftpAckNackMode.Metadata"/>).
    /// Padrão: <c>/archive/success</c>.
    /// </summary>
    public string ArchiveSuccessDirectory { get; set; } = "/archive/success";

    /// <summary>
    /// Diretório de arquivo para arquivos rejeitados (modo <see cref="SftpAckNackMode.Metadata"/>).
    /// Padrão: <c>/archive/error</c>.
    /// </summary>
    public string ArchiveErrorDirectory { get; set; } = "/archive/error";

    /// <summary>
    /// Diretório onde os arquivos de marcador ACK/NACK são criados
    /// (modo <see cref="SftpAckNackMode.MarkerFile"/>). Padrão: <c>/ack</c>.
    /// </summary>
    public string AckMarkerDirectory { get; set; } = "/ack";

    /// <summary>
    /// Mecanismo de ACK/NACK utilizado. Padrão: <see cref="SftpAckNackMode.Metadata"/> (mover arquivo).
    /// Escolha <see cref="SftpAckNackMode.MarkerFile"/> quando o servidor MFT exigir arquivos de marcador.
    /// </summary>
    public SftpAckNackMode AckNackMode { get; set; } = SftpAckNackMode.Metadata;
}
