using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nuuvify.CommonPack.Security;

/// <summary>
/// Extensões para integração do esquema de autenticação por API key com OpenAPI/Swagger.
/// </summary>
public static class ApiKeyOpenApiExtensions
{
    /// <summary>
    /// Registra o esquema de autenticação por API key no gerador OpenAPI.
    ///
    /// Use este método quando a aplicação utiliza Swashbuckle e precisa documentar endpoints
    /// protegidos por API key.
    ///
    /// Exemplo de uso:
    /// <code>
    /// builder.Services.AddSwaggerGen(options =>
    /// {
    ///     options.AddApiKeyOpenApiSecurity();
    /// });
    /// </code>
    /// </summary>
    /// <param name="swaggerGenOptions">Configuração do gerador Swagger.</param>
    /// <param name="headerName">Nome do header HTTP para autenticação por API key. Padrão: X-API-Key.</param>
    /// <remarks>
    /// Este método registra a definição de segurança no esquema OpenAPI sem acoplar
    /// a biblioteca Security a dependências de ASP.NET Core ou OpenAPI.
    /// O layout deve estar presente apenas se a aplicação usar Swashbuckle.
    /// </remarks>
    public static void AddApiKeyOpenApiSecurity(
        this SwaggerGenOptions swaggerGenOptions,
        string? headerName = null)
    {
        if (swaggerGenOptions is null)
        {
            throw new ArgumentNullException(nameof(swaggerGenOptions));
        }

        headerName = string.IsNullOrWhiteSpace(headerName)
            ? ApiKeyAuthenticationDefaults.AuthenticationScheme
            : headerName;

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = headerName,
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Autenticação por chave de API. Forneça o valor no header HTTP.",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = ApiKeyAuthenticationDefaults.AuthenticationScheme
            }
        };

        swaggerGenOptions.AddSecurityDefinition(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            securityScheme);

        swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                securityScheme,
                Array.Empty<string>()
            }
        });
    }
}
