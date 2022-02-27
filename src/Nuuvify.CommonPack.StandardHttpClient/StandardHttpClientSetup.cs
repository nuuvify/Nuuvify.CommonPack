using System;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.StandardHttpClient
{
    public static class StandardHttpClientSetup
    {
        /// <summary>
        /// Inclua esse setup antes das demais configurações de HttpClient <br/>
        /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
        ///     "AppConfig:AppURLs:UrlCredentialApi" <br/>
        ///     "AppConfig:AppURLs:UrlCredentialApiToken"
        /// </summary>
        /// <remarks>
        ///  O CwsApi já é registrado automaticamente ao configurar essa classe
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddStandardHttpClientSetup(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddScoped<IStandardHttpClient, StandardHttpClient>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddTransient<CredentialToken>();

            AddServiceCredentialRegister(services, configuration);

        }

        /// <summary>
        /// Inclua esse setup antes das demais configurações de HttpClient <br/>
        /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
        ///     "AppConfig:AppURLs:UrlCredentialApi" <br/>
        ///     "AppConfig:AppURLs:UrlCredentialApiToken"
        /// </summary>
        /// <remarks>
        ///  O CwsApi já é registrado automaticamente ao configurar essa classe
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddStandardHttpClientSetupSingleton(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddSingleton<IStandardHttpClient, StandardHttpClient>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<CredentialToken>();

            AddServiceCredentialRegister(services, configuration);

        }


        private static void AddServiceCredentialRegister(IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddHttpClient("CredentialApi", client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("AppConfig:AppURLs:UrlCredentialApi")?.Value);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyWithTokenHandlers(services, retryTotal: 2, breakDurationMilliSeconds: 2000);

        }


    }
}
