using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Security;

internal sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var presentedKey = headerValues.ToString();
        var matched = Options.ValidKeys.Any(validKey => KeysEqual(presentedKey, validKey));
        if (!matched)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        var identity = new ClaimsIdentity(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);
        identity.AddClaim(new Claim(ApiKeyAuthenticationDefaults.ClaimType, Options.HeaderName));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool KeysEqual(string presentedKey, string configuredKey)
    {
        if (presentedKey is null || configuredKey is null)
            return false;

        var presentedBytes = Encoding.UTF8.GetBytes(presentedKey);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        return CryptographicOperations.FixedTimeEquals(presentedBytes, configuredBytes);
    }
}
