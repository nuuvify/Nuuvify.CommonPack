using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.MftMailbox.Protocols;
using Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.Sftp;

/// <summary>
/// Extensões de <see cref="IServiceCollection"/> para registrar o cliente SFTP do MftMailbox.
/// </summary>
/// <remarks>
/// Deve ser chamado após <c>AddMftMailboxCore</c>. Exemplo:
/// <code>
/// services.AddMftMailboxCore();
/// services.AddMftMailboxSftp(sftp =&gt;
/// {
///     sftp.Host = "sftp.parceiro.com";
///     sftp.Port = 22;
///     sftp.Username = "usuario";
///     sftp.PrivateKeyPath = "/run/secrets/sftp_key";
/// });
/// </code>
/// O cliente é registrado como Singleton (<see cref="SftpMftMailboxClient"/>) e exposto
/// como <see cref="IProtocolMftClient"/> para ser resolvido pela <c>MftClientFactory</c>.
/// </remarks>
public static class SftpMftMailboxSetup
{
    /// <summary>
    /// Registra o cliente SFTP e suas opções no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configureOptions">Delegate obrigatório para configurar <see cref="SftpMftMailboxOptions"/>.</param>
    /// <returns>A mesma <see cref="IServiceCollection"/> para encadeamento de chamadas.</returns>
    public static IServiceCollection AddMftMailboxSftp(this IServiceCollection services, Action<SftpMftMailboxOptions> configureOptions)
    {
        _ = services.Configure(configureOptions);
        _ = services.AddSingleton<IProtocolMftClient, SftpMftMailboxClient>();
        return services;
    }
}
