using Microsoft.AspNetCore.Authentication;

namespace Nuuvify.CommonPack.Security;

/// <summary>
/// Configura a autenticação por API key.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    private static readonly char[] s_lineBreakCharacters = { '\r', '\n' };

    /// <summary>
    /// Obtém ou define o nome do header que transporta a API key.
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Obtém ou define as credenciais válidas.
    /// </summary>
    public IReadOnlyCollection<string> ValidKeys { get; set; } = Array.Empty<string>();

    internal bool IsValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(HeaderName) || HeaderName.IndexOfAny(s_lineBreakCharacters) >= 0)
        {
            errorMessage = "O nome do header da API key é obrigatório e não pode conter quebra de linha.";
            return false;
        }

        if (ValidKeys is null || ValidKeys.Count == 0 || ValidKeys.Any(string.IsNullOrEmpty))
        {
            errorMessage = "É necessário configurar ao menos uma API key não vazia.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
