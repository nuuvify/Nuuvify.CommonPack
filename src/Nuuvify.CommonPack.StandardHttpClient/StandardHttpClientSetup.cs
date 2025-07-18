using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;

namespace Nuuvify.CommonPack.StandardHttpClient;

public static class StandardHttpClientSetup
{

    /// <summary>
    /// Numero de vezes de tentativas de acesso a um serviço
    /// </summary>
    /// <value></value>
    public static int RetryTotal { get; set; } = 2;
    /// <summary>
    /// Tempo minimo para primeira tentativa, a partir da segunda sera feito uma escala
    /// </summary>
    /// <value></value>
    public static int BreakDurationMilliSeconds { get; set; } = 2000;


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

    ///<inheritdoc cref="AddStandardHttpClientSetup"/>
    public static void AddStandardHttpClientSetupSingleton(this IServiceCollection services,
        IConfiguration configuration, bool registerCredential = true)
    {

        services.AddSingleton<IStandardHttpClient, StandardHttpClient>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<CredentialToken>();

        if (registerCredential)
            AddServiceCredentialRegister(services, configuration);

    }

    ///<inheritdoc cref="AddStandardHttpClientSetup"/>
    public static void AddStandardHttpClientSetupAddTransient(this IServiceCollection services,
        IConfiguration configuration, bool registerCredential = true)
    {

        services.AddTransient<IStandardHttpClient, StandardHttpClient>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<CredentialToken>();

        if (registerCredential)
            AddServiceCredentialRegister(services, configuration);

    }

    /// <summary>
    /// Registra CredentialApi e IHttpContextAccessor. <br/>
    /// Use .ConfigurePrimaryHttpMessageHandler para registro com proxy e demais parametros do HttpClient
    /// Você precisa incluir as tags no seu arquivo appsettings.json <br/>
    ///     "AppConfig:AppURLs:UrlLoginApi" <br/>
    ///     "AppConfig:AppURLs:UrlLoginApiToken" 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="httpclientName"></param>
    public static IHttpClientBuilder AddServiceCredentialRegister(this IServiceCollection services,
        IConfiguration configuration, string httpclientName = "CredentialApi")
    {

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


        return services.AddHttpClient(httpclientName, client =>
        {
            client.BaseAddress = new Uri(configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyWithTokenHandlers(services, retryTotal: RetryTotal, breakDurationMilliSeconds: BreakDurationMilliSeconds);

    }


}
