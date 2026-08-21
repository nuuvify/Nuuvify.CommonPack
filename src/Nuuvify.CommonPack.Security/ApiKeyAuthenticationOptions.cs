using Microsoft.AspNetCore.Authentication;

namespace Nuuvify.CommonPack.Security;

/// <summary>
/// Configura a autenticação por API key.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Obtém ou define o nome do header que transporta a API key.
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Obtém ou define as credenciais válidas.
    /// </summary>
    public IReadOnlyCollection<string> ValidKeys { get; set; } = Array.Empty<string>();
}
