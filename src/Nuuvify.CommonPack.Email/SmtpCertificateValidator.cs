using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Nuuvify.CommonPack.Email.Abstraction;

namespace Nuuvify.CommonPack.Email;

/// <summary>
/// Valida o certificado TLS do servidor SMTP de acordo com as políticas definidas em <see cref="EmailServerConfiguration"/>.
/// </summary>
internal sealed class SmtpCertificateValidator
{
    // Erros aceitos quando o certificado é aceito por pinning: cadeia não confiável no SO ou revogação não verificável.
    // Assinatura inválida, validade expirada, certificado revogado etc. continuam rejeitando o certificado.
    private const X509ChainStatusFlags PinningToleratedChainErrors =
        X509ChainStatusFlags.UntrustedRoot |
        X509ChainStatusFlags.RevocationStatusUnknown |
        X509ChainStatusFlags.OfflineRevocation;

    private readonly EmailServerConfiguration _configuration;
    private readonly HashSet<string> _allowedThumbprints;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SmtpCertificateValidator"/>.
    /// </summary>
    /// <param name="configuration">Configuração do servidor de e-mail com as políticas de validação.</param>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="configuration"/> é nulo.</exception>
    public SmtpCertificateValidator(EmailServerConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _allowedThumbprints = new HashSet<string>(
            (configuration.AllowedCertificateThumbprints ?? Array.Empty<string>())
                .Where(thumbprint => !string.IsNullOrWhiteSpace(thumbprint))
                .Select(NormalizeThumbprint),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indica se a configuração exige um callback de validação diferente do padrão do MailKit.
    /// </summary>
    public bool HasCustomPolicy => !_configuration.EnableStrictSslValidation || _configuration.EnableCertificatePinning;

    /// <summary>
    /// Mensagem com o motivo da última decisão de validação, para registro em log.
    /// </summary>
    public string LastValidationMessage { get; private set; }

    /// <summary>
    /// Valida o certificado apresentado pelo servidor SMTP.
    /// </summary>
    /// <param name="sender">Origem da validação (não utilizado).</param>
    /// <param name="certificate">Certificado apresentado pelo servidor.</param>
    /// <param name="chain">Cadeia construída para o certificado.</param>
    /// <param name="sslPolicyErrors">Erros de política SSL detectados.</param>
    /// <returns><c>true</c> se o certificado for aceito pelas políticas configuradas; caso contrário, <c>false</c>.</returns>
    public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (certificate is null)
        {
            return Reject("Certificado do servidor SMTP não informado");
        }

        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return Accept($"Certificado SMTP válido: {certificate.Subject}");
        }

        if (!_configuration.EnableStrictSslValidation && !_configuration.EnableCertificatePinning)
        {
            return Accept($"Validação SSL estrita desabilitada - certificado SMTP aceito sem validação: {certificate.Subject} ({sslPolicyErrors})");
        }

        if (_configuration.EnableCertificatePinning && _allowedThumbprints.Count > 0)
        {
            return ValidatePinning(certificate, chain, sslPolicyErrors);
        }

        return Reject($"Certificado SMTP rejeitado: {certificate.Subject} ({sslPolicyErrors})");
    }

    private bool ValidatePinning(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors || chain is null)
        {
            return Reject($"Certificado SMTP rejeitado no pinning por erro não tolerado: {certificate.Subject} ({sslPolicyErrors})");
        }

        var leafThumbprint = new X509Certificate2(certificate).Thumbprint;
        var rootThumbprint = chain.ChainElements.Count > 0
            ? chain.ChainElements[chain.ChainElements.Count - 1].Certificate.Thumbprint
            : null;

        if (!_allowedThumbprints.Contains(leafThumbprint) &&
            (rootThumbprint is null || !_allowedThumbprints.Contains(rootThumbprint)))
        {
            return Reject($"Certificado SMTP rejeitado - thumbprint não permitido. Servidor: {leafThumbprint}, raiz: {rootThumbprint}");
        }

        var chainErrors = chain.ChainElements
            .Cast<X509ChainElement>()
            .SelectMany(element => element.ChainElementStatus)
            .Select(status => status.Status)
            .Aggregate(X509ChainStatusFlags.NoError, (current, status) => current | status);

        if ((chainErrors & ~PinningToleratedChainErrors) != X509ChainStatusFlags.NoError)
        {
            return Reject($"Certificado SMTP rejeitado no pinning por erro de cadeia não tolerado: {certificate.Subject} ({chainErrors})");
        }

        return Accept($"Certificado SMTP aceito via certificate pinning: {certificate.Subject} (raiz: {rootThumbprint})");
    }

    private bool Accept(string message)
    {
        LastValidationMessage = message;
        return true;
    }

    private bool Reject(string message)
    {
        LastValidationMessage = message;
        return false;
    }

    private static string NormalizeThumbprint(string thumbprint)
        => thumbprint.Replace(":", string.Empty).Replace(" ", string.Empty).Trim();
}
