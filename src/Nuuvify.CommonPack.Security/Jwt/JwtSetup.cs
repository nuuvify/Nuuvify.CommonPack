using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Helpers;
using Nuuvify.CommonPack.Security.Resources;

namespace Nuuvify.CommonPack.Security.Jwt;

public static class UserSecuritySetup
{

    /// <summary>
    /// IMPORTANTE: É recomendado que você inclua os parametros do JwtTokenOptions em um arquivo ou serviço de Secret/Vault
    /// A classe JwtTokenOptions esta injetada e pode ser recuperada a qualquer momento no projeto
    /// Esse setup já injeta a interface IUserAuthenticated automaticamente
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="jwtAppSettingsKey"></param>
    public static void AddSecuritySetup(this IServiceCollection services, IConfiguration configuration, string jwtAppSettingsKey = null)
    {

        if (services is null)
            throw new ArgumentNullException(nameof(services), MsgSecurityJwt.ResourceManager.GetString("ServiceNull", CultureInfo.CurrentCulture));

        if (configuration is null)
            throw new ArgumentNullException(MsgSecurityJwt.ResourceManager.GetString("ConfigurationNull", CultureInfo.CurrentCulture));

        var appSettingsSection = configuration.GetSection(jwtAppSettingsKey ?? "JwtTokenOptions");
        if (appSettingsSection?.Key is null)
        {
            throw new ArgumentNullException(MsgSecurityJwt.ResourceManager.GetString("ConfigurationNull", CultureInfo.CurrentCulture));
        }
        else
        {
            _ = services.Configure<JwtTokenOptions>(appSettingsSection);
        }

        var appSettings = appSettingsSection.Get<JwtTokenOptions>() ?? throw new ArgumentNullException(MsgSecurityJwt.ResourceManager.GetString("ConfigurationNull", CultureInfo.CurrentCulture));
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        _ = services.AddSingleton<IAuthorizationHandler, ControllerCustomAuthorizationHandler>();
        services.TryAddScoped<IUserAuthenticated, UserAuthenticated>();

        _ = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(bearerOptions =>
        {

            bearerOptions.RequireHttpsMetadata = true;
            bearerOptions.SaveToken = true;

            bearerOptions.TokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuer = true,
                ValidIssuer = appSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = appSettings.Audience,

                // Valida a assinatura de um token recebido
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = appSettings.GetSymmetricSecurityKey(),

                // Verifica se um token recebido ainda é válido
                RequireExpirationTime = true,
                ValidateLifetime = true,

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                ClockSkew = TimeSpan.Zero
            };

        });

    }

}
