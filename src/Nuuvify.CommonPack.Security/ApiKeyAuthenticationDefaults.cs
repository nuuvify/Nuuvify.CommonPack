namespace Nuuvify.CommonPack.Security;

/// <summary>
/// Define os valores padrão do esquema de autenticação por API key.
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    /// <summary>
    /// Nome padrão do esquema de autenticação.
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// Tipo da claim que identifica uma autenticação por API key.
    /// </summary>
    public const string ClaimType = "urn:nuuvify:security:api-key";
}
