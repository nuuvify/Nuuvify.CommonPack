using Nuuvify.CommonPack.MftMailbox.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.Http.Configuration;

/// <summary>
/// Opções de configuração do cliente HTTP para transferência MFT via API REST.
/// </summary>
/// <remarks>
/// Configurado via <c>services.AddMftMailboxHttp(http =&gt; { ... })</c>.
/// Todos os caminhos (<see cref="UploadPath"/>, <see cref="DownloadPath"/>, etc.) são relativos
/// a <see cref="BaseUrl"/>. O cliente suporta Bearer Token e mutual TLS para integrações seguras.
/// Não armazene tokens em texto claro; use variáveis de ambiente ou Azure Key Vault.
/// </remarks>
public sealed class HttpMftMailboxOptions
{
    /// <summary>
    /// URL base da API MFT (ex.: <c>https://mft.parceiro.com</c>). Obrigatória.
    /// Configurada automaticamente como <see cref="HttpClient.BaseAddress"/>.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Caminho do endpoint de upload (envio de arquivos). Padrão: <c>/mailbox/upload</c>.
    /// Recebe <c>multipart/form-data</c> com os campos <c>file</c>, <c>itemId</c>,
    /// <c>integrationKey</c> e <c>correlationId</c>.
    /// </summary>
    public string UploadPath { get; set; } = "/mailbox/upload";

    /// <summary>
    /// Caminho do endpoint de download (recepção de arquivo por caminho remoto). Padrão: <c>/mailbox/download</c>.
    /// Recebe o parâmetro de query <c>remotePath</c> (URL-encoded).
    /// </summary>
    public string DownloadPath { get; set; } = "/mailbox/download";

    /// <summary>
    /// Caminho do endpoint de listagem de arquivos disponíveis. Padrão: <c>/mailbox/list</c>.
    /// Deve retornar JSON compatível com <c>MailboxListResponse</c> (lista de itens com <c>itemId</c>, <c>fileName</c> e <c>remotePath</c>).
    /// </summary>
    public string ListPath { get; set; } = "/mailbox/list";

    /// <summary>
    /// Caminho do endpoint de consulta de status. Padrão: <c>/mailbox/status</c>.
    /// Recebe os parâmetros de query <c>integrationKey</c> e <c>itemId</c>.
    /// Deve retornar JSON compatível com <see cref="Nuuvify.CommonPack.MftMailbox.Abstraction.Models.TransferStatus"/> ou 404.
    /// </summary>
    public string StatusPath { get; set; } = "/mailbox/status";

    /// <summary>
    /// Caminho do endpoint de ACK/NACK. Padrão: <c>/mailbox/acknack</c>.
    /// Recebe JSON com <see cref="Nuuvify.CommonPack.MftMailbox.Abstraction.Models.AckNackCommand"/>.
    /// </summary>
    public string AckNackPath { get; set; } = "/mailbox/acknack";

    /// <summary>
    /// Token Bearer para autenticação HTTP. Quando definido, é adicionado como
    /// <c>Authorization: Bearer &lt;token&gt;</c> em todas as requisições.
    /// Não informe junto com mutual TLS; escolha um mecanismo de autenticação.
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// Quando <see langword="true"/>, habilita mutual TLS (mTLS) na requisição.
    /// Configure o certificado do cliente no <see cref="HttpClient"/> antes de registrar o cliente.
    /// </summary>
    public bool UseMutualTls { get; set; }

    /// <summary>
    /// Número máximo de tentativas de polling de status após o upload.
    /// O polling aguarda o servidor confirmar o processamento antes de marcar como concluído.
    /// Padrão: 10 tentativas.
    /// </summary>
    public int StatusPollingMaxAttempts { get; set; } = 10;

    /// <summary>
    /// Atraso base para o backoff exponencial do polling de status.
    /// O atraso dobra a cada tentativa, limitado a 30 segundos.
    /// Padrão: 2 segundos.
    /// </summary>
    public TimeSpan StatusPollingBaseDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Estratégia de ordenação aplicada à lista inbound retornada pelo endpoint de listagem.
    /// Padrão: <see cref="InboundFileOrdering.None"/>.
    /// </summary>
    public InboundFileOrdering InboundFileOrdering { get; set; } = InboundFileOrdering.None;

    /// <summary>
    /// Quando <see langword="true"/>, metadados do envelope/item são enviados como campos
    /// adicionais no multipart do upload.
    /// </summary>
    public bool IncludeMetadataInUploadForm { get; set; }

    /// <summary>
    /// Prefixo dos campos de metadados do envelope enviados no multipart.
    /// Exemplo de campo final: <c>envMeta:chave</c>.
    /// </summary>
    public string EnvelopeMetadataFieldPrefix { get; set; } = "envMeta:";

    /// <summary>
    /// Prefixo dos campos de metadados do item enviados no multipart.
    /// Exemplo de campo final: <c>itemMeta:chave</c>.
    /// </summary>
    public string ItemMetadataFieldPrefix { get; set; } = "itemMeta:";
}
