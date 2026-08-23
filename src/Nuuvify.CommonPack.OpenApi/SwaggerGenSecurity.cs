using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Nuuvify.CommonPack.OpenApi;

public static class SwaggerGenSecurity
{
    /// <summary>
    /// Registra no Swagger o esquema de autenticação por API key.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="headerName">Nome do header que transporta a credencial.</param>
    public static void ConfigurationApiKey(this IServiceCollection services, string headerName = "X-API-Key")
    {
        _ = services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API key usada para autenticar a requisição.",
                Name = headerName,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    new List<string>()
                }
            });
        });
    }

    public static void Configuration(this IServiceCollection services)
    {
        _ = services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                                        Enter 'Bearer' [space] and then your token in the text input below.
                                        Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                Scheme = "Bearer",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });

        });
    }

}
