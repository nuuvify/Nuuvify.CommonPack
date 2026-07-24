using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Services;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis;

/// <summary>
/// Extensões de <see cref="IServiceCollection"/> para registrar suporte Redis no MftMailbox.
/// </summary>
/// <remarks>
/// Adiciona implementações distribuídas via Redis para idempotência, cache de status
/// e auditoria em stream. Requer que <see cref="IConnectionMultiplexer"/> já esteja
/// registrado no container de DI.
///
/// Uso recomendado:
/// <code>
/// // 1. Registrar IConnectionMultiplexer
/// services.AddSingleton&lt;IConnectionMultiplexer&gt;(_ =&gt;
///     ConnectionMultiplexer.Connect("localhost:6379"));
///
/// // 2. Registrar core MftMailbox
/// services.AddMftMailboxCore(opts =>
/// {
///     opts.MaxBatchSize = 100;
///     opts.Retry.MaxRetries = 5;
/// });
///
/// // 3. Registrar Redis para MftMailbox (substitui in-memory)
/// services.AddMftMailboxRedis(opts =>
/// {
///     opts.IdempotencyTtl = TimeSpan.FromHours(24);
///     opts.StatusCacheTtl = TimeSpan.FromMinutes(5);
///     opts.EnableAuditStream = true;
/// });
///
/// // 4. Registrar protocolos específicos
/// services.AddMftMailboxSftp(/* ... */);
/// services.AddMftMailboxHttp(/* ... */);
/// </code>
/// </remarks>
public static class RedisMftMailboxSetup
{
    /// <summary>
    /// Registra serviços Redis para MftMailbox no container de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configureOptions">
    /// Delegate opcional para configurar <see cref="RedisMftMailboxOptions"/>.
    /// Quando <see langword="null"/>, valores padrão são usados.
    /// </param>
    /// <returns>A mesma <see cref="IServiceCollection"/> para encadeamento de chamadas.</returns>
    /// <remarks>
    /// Registra os seguintes serviços como Singleton:
    /// <list type="bullet">
    /// <item><see cref="IMftIdempotencyStore"/> → <see cref="RedisMftIdempotencyStore"/> (substitui in-memory).</item>
    /// <item><see cref="IMftStatusClient"/> → <see cref="RedisCachedStatusClient"/> (decora cliente original).</item>
    /// <item><see cref="ITransferAuditSink"/> → <see cref="RedisAuditStreamSink"/> (se EnableAuditStream=true).</item>
    /// <item><see cref="RedisStatusSerializer"/> → Serializador para status (Singleton).</item>
    /// <item><see cref="RedisAuditSerializer"/> → Serializador para auditoria (Singleton).</item>
    /// </list>
    ///
    /// <c>AddMftMailboxRedis</c> deve ser chamado APÓS <c>AddMftMailboxCore</c>,
    /// de modo que as substituições de Singleton funcionem corretamente.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Lançado quando <paramref name="services"/> é <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Lançado se <see cref="IConnectionMultiplexer"/> não estiver registrado no DI.
    /// </exception>
    public static IServiceCollection AddMftMailboxRedis(
        this IServiceCollection services,
        Action<RedisMftMailboxOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        configureOptions ??= _ => { };

        // Validar que Redis já está registrado
        if (!services.Any(sd => sd.ServiceType == typeof(IConnectionMultiplexer)))
        {
            throw new InvalidOperationException(
                "IConnectionMultiplexer não está registrado no container de DI. " +
                "Registre Redis antes de chamar AddMftMailboxRedis. " +
                "Exemplo: services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(\"localhost:6379\"));");
        }

        // Configurar opções
        _ = services.Configure(configureOptions);

        // Registrar serializadores
        _ = services.AddSingleton<RedisStatusSerializer>();
        _ = services.AddSingleton<RedisAuditSerializer>();

        // Substituir IMftIdempotencyStore com Redis (critério produção)
        _ = services.AddSingleton<IMftIdempotencyStore, RedisMftIdempotencyStore>();

        // Decorar IMftStatusClient com cache Redis (opcional, configurable)
        var options = new RedisMftMailboxOptions();
        configureOptions(options);

        if (options.EnableStatusCache)
        {
            // Nota: Esta implementação assume que IMftStatusClient já está registrado
            // (e.g., por AddMftMailboxSftp ou AddMftMailboxHttp).
            // Esta factory envolve o cliente existente com um camada de cache Redis.

            // Para preservar o cliente original, guardamos uma referência temporária
            var mftStatusClientDescriptor = services.LastOrDefault(sd => sd.ServiceType == typeof(IMftStatusClient));

            if (mftStatusClientDescriptor is not null)
            {
                // Remover o registro anterior (será re-adicionado via factory)
                _ = services.Remove(mftStatusClientDescriptor);

                // Registrar o cliente com decorador de cache
                _ = services.AddSingleton<IMftStatusClient>(sp =>
                {
                    var innerClient = CreateInnerStatusClient(sp, mftStatusClientDescriptor);

                    return new RedisCachedStatusClient(
                        innerClient,
                        sp.GetRequiredService<IConnectionMultiplexer>(),
                        sp.GetRequiredService<IOptions<RedisMftMailboxOptions>>(),
                        sp.GetRequiredService<RedisStatusSerializer>(),
                        sp.GetRequiredService<ILogger<RedisCachedStatusClient>>());
                });
            }
        }

        // Substituir ITransferAuditSink com Redis Stream (se habilitado)
        if (options.EnableAuditStream)
        {
            _ = services.AddSingleton<ITransferAuditSink, RedisAuditStreamSink>();
        }

        return services;
    }

    private static IMftStatusClient CreateInnerStatusClient(IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ImplementationInstance is IMftStatusClient implementationInstance)
        {
            return implementationInstance;
        }

        if (serviceDescriptor.ImplementationFactory is not null)
        {
            var implementation = serviceDescriptor.ImplementationFactory(serviceProvider);
            return implementation as IMftStatusClient
                ?? throw new InvalidOperationException("A factory registrada para IMftStatusClient não retornou uma implementação válida.");
        }

        if (serviceDescriptor.ImplementationType is not null)
        {
            return (IMftStatusClient)ActivatorUtilities.CreateInstance(serviceProvider, serviceDescriptor.ImplementationType);
        }

        throw new InvalidOperationException("Não foi possível resolver o registro original de IMftStatusClient para aplicar o decorador Redis.");
    }
}
