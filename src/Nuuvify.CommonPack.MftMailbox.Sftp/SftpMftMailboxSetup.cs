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
/// O cliente é registrado como <see cref="IProtocolMftClient"/> transiente keyed por
/// <see cref="Nuuvify.CommonPack.MftMailbox.Abstraction.Models.MftProtocol.Sftp"/>.
/// A reutilização de instância por protocolo é controlada explicitamente pela <c>MftClientFactory</c>.
/// </remarks>
public static class SftpMftMailboxSetup
{
    /// <summary>
    /// Registra o cliente SFTP e suas opções no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configureOptions">Delegate obrigatório para configurar <see cref="SftpMftMailboxOptions"/>.</param>
    /// <returns>A mesma <see cref="IServiceCollection"/> para encadeamento de chamadas.</returns>
    /// <exception cref="ArgumentNullException">Lançado quando <paramref name="services"/> ou <paramref name="configureOptions"/> são <see langword="null"/>.</exception>
    public static IServiceCollection AddMftMailboxSftp(this IServiceCollection services, Action<SftpMftMailboxOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        _ = services.Configure(configureOptions);
        _ = services.AddKeyedTransient<IProtocolMftClient, SftpMftMailboxClient>(Nuuvify.CommonPack.MftMailbox.Abstraction.Models.MftProtocol.Sftp);
        return services;
    }
}
