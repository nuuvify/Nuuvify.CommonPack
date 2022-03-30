using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;

namespace Nuuvify.CommonPack.StandardHttpClient
{
    public static class StandardHttpClientSetup
    {
        /// <summary>
        /// Inclua esse setup antes das demais configurações de HttpClient <br/>
        /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
        ///     "AppConfig:AppURLs:UrlLoginApi" <br/>
        ///     "AppConfig:AppURLs:UrlLoginApiToken"
        /// </summary>
        /// <remarks>
        ///  A CredentialApi já é registrado automaticamente ao configurar essa classe <br/>
        ///  Assim como: IStandardHttpClient, ITokenService e CredentialToken
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="webProxy">Sera utilizado para configurar CredentialApi, voce pode utilizar WebRequest.DefaultWebProxy</param>
        public static void AddStandardHttpClientSetup(this IServiceCollection services,
            IConfiguration configuration, IWebProxy webProxy = null)
        {
            services.AddScoped<IStandardHttpClient, StandardHttpClient>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddTransient<CredentialToken>();

            AddServiceCredentialRegister(services, configuration, webProxy);

        }

        /// <summary>
        /// Inclua esse setup antes das demais configurações de HttpClient <br/>
        /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
        ///     "AppConfig:AppURLs:UrlLoginApi" <br/>
        ///     "AppConfig:AppURLs:UrlLoginApiToken"
        /// </summary>
        /// <remarks>
        ///  A CredentialApi já é registrado automaticamente ao configurar essa classe
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="webProxy">Sera utilizado para configurar CredentialApi, voce pode utilizar WebRequest.DefaultWebProxy</param>
        public static void AddStandardHttpClientSetupSingleton(this IServiceCollection services,
            IConfiguration configuration, IWebProxy webProxy = null)
        {

            services.AddSingleton<IStandardHttpClient, StandardHttpClient>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<CredentialToken>();

            AddServiceCredentialRegister(services, configuration, webProxy);

        }


        private static void AddServiceCredentialRegister(IServiceCollection services,
            IConfiguration configuration, IWebProxy webProxy = null)
        {

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddHttpClient("CredentialApi", client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyWithTokenHandlers(services, retryTotal: 2, breakDurationMilliSeconds: 2000, webProxy: webProxy);

        }


    }
}
