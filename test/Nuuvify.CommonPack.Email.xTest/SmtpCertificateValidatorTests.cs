using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Shouldly;

namespace Nuuvify.CommonPack.Email.xTest;

/// <summary>
/// Testes unitários para <see cref="SmtpCertificateValidator"/> e para a aplicação das políticas TLS no <see cref="SmtpClient"/>.
/// </summary>
public sealed class SmtpCertificateValidatorTests : IDisposable
{
    private readonly X509Certificate2 _root;
    private readonly X509Certificate2 _server;
    private readonly X509Chain _untrustedChain;

    public SmtpCertificateValidatorTests()
    {
        _root = CreateRootCa();
        _server = CreateServerCertificate(_root, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        _untrustedChain = BuildChain(_server, _root);
    }

    public void Dispose()
    {
        _untrustedChain.Dispose();
        _server.Dispose();
        _root.Dispose();
    }

    private static EmailServerConfiguration Config(bool strict = true, bool pinning = false, params string[] thumbprints)
        => new()
        {
            ServerHost = "smtp.test.local",
            EnableStrictSslValidation = strict,
            EnableCertificatePinning = pinning,
            AllowedCertificateThumbprints = thumbprints
        };

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithoutSslErrors_ShouldAccept()
    {
        var sut = new SmtpCertificateValidator(Config());

        sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.None).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_StrictWithoutPinning_ShouldRejectUntrustedChain()
    {
        var sut = new SmtpCertificateValidator(Config());

        sut.HasCustomPolicy.ShouldBeFalse();
        sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_StrictDisabledWithoutPinning_ShouldAcceptAnyCertificate()
    {
        var sut = new SmtpCertificateValidator(Config(strict: false));

        sut.HasCustomPolicy.ShouldBeTrue();
        sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_PinningRootOrServerThumbprint_ShouldAcceptUntrustedChain()
    {
        var formattedRoot = string.Join(":", Enumerable.Range(0, _root.Thumbprint.Length / 2).Select(i => _root.Thumbprint.Substring(i * 2, 2))).ToLowerInvariant();

        foreach (var thumbprint in new[] { _root.Thumbprint, formattedRoot, _server.Thumbprint })
        {
            var sut = new SmtpCertificateValidator(Config(pinning: true, thumbprints: thumbprint));

            sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeTrue(thumbprint);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_PinningWithUnknownThumbprint_ShouldReject()
    {
        var sut = new SmtpCertificateValidator(Config(pinning: true, thumbprints: new string('A', 40)));

        sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeFalse();
        sut.LastValidationMessage.ShouldContain("thumbprint não permitido");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_PinnedRootWithNonToleratedErrors_ShouldReject()
    {
        var sut = new SmtpCertificateValidator(Config(pinning: true, thumbprints: _root.Thumbprint));
        using var expired = CreateServerCertificate(_root, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(-1));
        using var expiredChain = BuildChain(expired, _root);

        sut.Validate(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeFalse();
        sut.Validate(null, _server, null, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeFalse();
        sut.Validate(null, expired, expiredChain, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeFalse();
        sut.LastValidationMessage.ShouldContain(nameof(X509ChainStatusFlags.NotTimeValid));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateSmtpClient_ShouldApplyRevocationAndValidationPolicies()
    {
        var defaultConfig = Config();
        var pinningConfig = Config(pinning: true, thumbprints: _root.Thumbprint);
        pinningConfig.CheckCertificateRevocation = false;

        using var defaultClient = new Email(defaultConfig).CreateSmtpClient();
        var email = new Email(pinningConfig);
        using var pinningClient = email.CreateSmtpClient();

        defaultClient.CheckCertificateRevocation.ShouldBeTrue();
        defaultClient.ServerCertificateValidationCallback.ShouldBeNull();
        pinningClient.CheckCertificateRevocation.ShouldBeFalse();
        pinningClient.ServerCertificateValidationCallback(null, _server, _untrustedChain, SslPolicyErrors.RemoteCertificateChainErrors).ShouldBeTrue();
        email.LogMessage.ShouldContain(x => x.Message.Contains("certificate pinning"));
    }

    private static X509Certificate2 CreateRootCa()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=Nuuvify Test Root CA", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-60), DateTimeOffset.UtcNow.AddYears(5));
    }

    private static X509Certificate2 CreateServerCertificate(X509Certificate2 issuer, DateTimeOffset notBefore, DateTimeOffset notAfter)
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=smtp.test.local", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

        return request.Create(issuer, notBefore, notAfter, Guid.NewGuid().ToByteArray());
    }

    private static X509Chain BuildChain(X509Certificate2 certificate, X509Certificate2 root)
    {
        var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        _ = chain.ChainPolicy.ExtraStore.Add(root);
        _ = chain.Build(certificate);

        return chain;
    }
}
