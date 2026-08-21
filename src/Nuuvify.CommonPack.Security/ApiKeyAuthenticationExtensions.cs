using Microsoft.AspNetCore.Authentication;

namespace Nuuvify.CommonPack.Security;

/// <summary>
/// Contém extensões para registrar autenticação por API key.
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Registra o esquema de autenticação por API key.
    /// </summary>
    /// <param name="builder">Builder de autenticação.</param>
    /// <param name="configureOptions">Ação de configuração do esquema.</param>
    /// <returns>O mesmo builder para composição de registros.</returns>
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this AuthenticationBuilder builder,
        Action<ApiKeyAuthenticationOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            configureOptions);
    }
}
