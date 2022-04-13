using System;
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
        ///  Esse metodo tambem registra: IStandardHttpClient, ITokenService e CredentialToken
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="registerCredential">True = Registra CredentialApi ou False = Use AddServiceCredentialRegister</param>
        /// <returns></returns>
        public static void AddStandardHttpClientSetup(this IServiceCollection services,
            IConfiguration configuration, bool registerCredential = true)
        {
            services.AddScoped<IStandardHttpClient, StandardHttpClient>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddTransient<CredentialToken>();

            if (registerCredential)
                AddServiceCredentialRegister(services, configuration);

        }

        /// <summary>
        /// Inclua esse setup antes das demais configurações de HttpClient <br/>
        /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
        ///     "AppConfig:AppURLs:UrlLoginApi" <br/>
        ///     "AppConfig:AppURLs:UrlLoginApiToken"
        /// </summary>
        /// <remarks>
        ///  Esse metodo tambem registra: IStandardHttpClient, ITokenService e CredentialToken
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="registerCredential">True = Registra CredentialApi ou False = Use AddServiceCredentialRegister</param>
        /// <returns></returns>
        public static void AddStandardHttpClientSetupSingleton(this IServiceCollection services,
            IConfiguration configuration, bool registerCredential = true)
        {

            services.AddSingleton<IStandardHttpClient, StandardHttpClient>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<CredentialToken>();

            if (registerCredential)
                AddServiceCredentialRegister(services, configuration);

        }

        /// <summary>
        /// Registra CredentialApi e IHttpContextAccessor. <br/>
        /// Use .ConfigurePrimaryHttpMessageHandler para registro com proxy e demais parametros do HttpClient
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IHttpClientBuilder AddServiceCredentialRegister(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            return services.AddHttpClient("CredentialApi", client =>
            {
                client.BaseAddress = new Uri(configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyWithTokenHandlers(services, retryTotal: 2, breakDurationMilliSeconds: 2000);

        }


    }
}
