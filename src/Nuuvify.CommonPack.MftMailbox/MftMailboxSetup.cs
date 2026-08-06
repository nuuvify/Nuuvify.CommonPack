using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox;

/// <summary>
/// Extensões de <see cref="IServiceCollection"/> para registrar o núcleo do pacote MftMailbox.
/// </summary>
/// <remarks>
/// Sempre chame <c>AddMftMailboxCore</c> antes de registrar os protocolos específicos
/// (<c>AddMftMailboxSftp</c>, <c>AddMftMailboxHttp</c>). Exemplo completo:
/// <code>
/// services.AddMftMailboxCore(opt =&gt;
/// {
///     opt.MaxBatchSize = 100;
///     opt.Retry.MaxRetries = 5;
///     // Opcional: habilitar cache da factory para HTTPS
///     // opt.CachedProtocols.Add(MftProtocol.Https);
/// });
/// services.AddMftMailboxSftp(sftp =&gt;
/// {
///     sftp.Host = "sftp.parceiro.com";
///     sftp.Username = "usuario";
///     sftp.Password = "senha";
/// });
/// </code>
/// </remarks>
public static class MftMailboxSetup
{
    /// <summary>
    /// Registra os serviços núcleo do MftMailbox no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configureOptions">
    /// Delegate opcional para configurar <see cref="MftMailboxOptions"/>.
    /// Quando <see langword="null"/>, os valores padrão são usados.
    /// </param>
    /// <returns>A mesma <see cref="IServiceCollection"/> para encadeamento de chamadas.</returns>
    /// <remarks>
    /// Registra os seguintes serviços como Singleton:
    /// <list type="bullet">
    /// <item><see cref="IMftIdempotencyStore"/> → <see cref="InMemoryMftIdempotencyStore"/> (substitua por implementação persistente em produção).</item>
    /// <item><see cref="ITransferAuditSink"/> → <see cref="NullTransferAuditSink"/> (substitua por implementação de auditoria real).</item>
    /// <item><see cref="IMftClientFactory"/> → <see cref="MftClientFactory"/> (com cache explícito por protocolo definido em <see cref="MftMailboxOptions.CachedProtocols"/>).</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Lançado quando <paramref name="services"/> é <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddMftMailboxCore(this IServiceCollection services, Action<MftMailboxOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        configureOptions ??= _ => { };
        _ = services.Configure(configureOptions);

        _ = services.AddSingleton<IMftIdempotencyStore, InMemoryMftIdempotencyStore>();
        _ = services.AddSingleton<ITransferAuditSink, NullTransferAuditSink>();
        _ = services.AddSingleton<IMftClientFactory, MftClientFactory>();

        return services;
    }
}
