using System.Security.Authentication;

namespace Nuuvify.CommonPack.Email.Abstraction;

/// <summary>
/// Incluir a tag EmailConfig:EmailServerConfiguration no appsettings.json
/// Você pode injetar as configurações de email conforme exemplo:
/// <example>
/// <code>
///     services.AddScoped{IEmail, Email}().Configure{EmailServerConfiguration}(
///             configuration.GetSection("EmailConfig:EmailServerConfiguration"));
/// </code>
/// </example>
/// </summary>
public class EmailServerConfiguration
{

    public string ServerHost { get; set; }
    public int Port { get; set; }
    public SslProtocols Security { get; set; }
    public string AccountUserName { get; set; }
    public string AccountPassword { get; set; }

    /// <summary>
    /// Indica se a revogação do certificado do servidor SMTP (CRL/OCSP) deve ser verificada.
    /// Padrão <c>true</c> (comportamento padrão do MailKit).
    /// Defina <c>false</c> quando o ambiente não consegue obter as CRLs da PKI do servidor (ex.: proxy corporativo).
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = true;

    /// <summary>
    /// Indica se o certificado do servidor SMTP deve ser válido sem nenhum erro de política SSL.
    /// Padrão <c>true</c>. Quando <c>false</c> e <see cref="EnableCertificatePinning"/> também for <c>false</c>,
    /// qualquer certificado é aceito (use apenas em desenvolvimento).
    /// </summary>
    public bool EnableStrictSslValidation { get; set; } = true;

    /// <summary>
    /// Habilita a aceitação de certificados cuja cadeia não é confiável no sistema operacional,
    /// desde que o certificado do servidor ou a autoridade raiz da cadeia esteja em <see cref="AllowedCertificateThumbprints"/>.
    /// Padrão <c>false</c>.
    /// </summary>
    public bool EnableCertificatePinning { get; set; }

    /// <summary>
    /// Thumbprints (SHA-1, hexadecimal) do certificado do servidor ou da autoridade raiz aceitos quando
    /// <see cref="EnableCertificatePinning"/> está habilitado. Prefira o thumbprint da raiz, que não muda
    /// quando o certificado do servidor é renovado.
    /// </summary>
    public string[] AllowedCertificateThumbprints { get; set; } = Array.Empty<string>();

}