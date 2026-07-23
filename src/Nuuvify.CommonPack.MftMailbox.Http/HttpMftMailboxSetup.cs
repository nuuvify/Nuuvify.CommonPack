using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.MftMailbox.Http.Configuration;
using Nuuvify.CommonPack.MftMailbox.Protocols;

namespace Nuuvify.CommonPack.MftMailbox.Http;

/// <summary>
/// Extensões de <see cref="IServiceCollection"/> para registrar o cliente HTTP do MftMailbox.
/// </summary>
/// <remarks>
/// Deve ser chamado após <c>AddMftMailboxCore</c>. Exemplo:
/// <code>
/// services.AddMftMailboxCore();
/// services.AddMftMailboxHttp(http =&gt;
/// {
///     http.BaseUrl = "https://mft.parceiro.com";
///     http.BearerToken = Environment.GetEnvironmentVariable("MFT_TOKEN");
/// });
/// </code>
/// O <see cref="HttpMftMailboxClient"/> é registrado via <c>AddHttpClient</c> (gerenciado pelo
/// <c>IHttpClientFactory</c>) e exposto como <see cref="IProtocolMftClient"/> (Singleton)
/// para resolução pela <c>MftClientFactory</c>.
/// </remarks>
public static class HttpMftMailboxSetup
{
    /// <summary>
    /// Registra o cliente HTTP e suas opções no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configureOptions">Delegate obrigatório para configurar <see cref="HttpMftMailboxOptions"/>.</param>
    /// <returns>A mesma <see cref="IServiceCollection"/> para encadeamento de chamadas.</returns>
    public static IServiceCollection AddMftMailboxHttp(this IServiceCollection services, Action<HttpMftMailboxOptions> configureOptions)
    {
        _ = services.Configure(configureOptions);
        _ = services.AddHttpClient<HttpMftMailboxClient>();
        _ = services.AddSingleton<IProtocolMftClient>(provider => provider.GetRequiredService<HttpMftMailboxClient>());
        return services;
    }
}
