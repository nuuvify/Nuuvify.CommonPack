using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Services;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;

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
/// // 1. Registrar Redis (conexão existing)
/// services.AddStackExchangeRedisCache(opts => 
///     opts.Configuration = "localhost:6379");
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
    /// <exception cref="InvalidOperationException">
    /// Lançado se <see cref="IConnectionMultiplexer"/> não estiver registrado no DI.
    /// </exception>
    public static IServiceCollection AddMftMailboxRedis(
        this IServiceCollection services,
        Action<RedisMftMailboxOptions>? configureOptions = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Validar que Redis já está registrado
        if (!services.Any(sd => sd.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer)))
        {
            throw new InvalidOperationException(
                "IConnectionMultiplexer não está registrado no container de DI. " +
                "Registre Redis antes de chamar AddMftMailboxRedis. " +
                "Exemplo: services.AddStackExchangeRedisCache(opts => opts.Configuration = \"localhost:6379\");");
        }

        // Configurar opções
        if (configureOptions is null)
        {
            _ = services.Configure<RedisMftMailboxOptions>(_ => { });
        }
        else
        {
            _ = services.Configure(configureOptions);
        }

        // Registrar serializadores
        _ = services.AddSingleton<RedisStatusSerializer>();
        _ = services.AddSingleton<RedisAuditSerializer>();

        // Substituir IMftIdempotencyStore com Redis (critério produção)
        _ = services.AddSingleton<IMftIdempotencyStore, RedisMftIdempotencyStore>();

        // Decorar IMftStatusClient com cache Redis (opcional, configurable)
        var options = new RedisMftMailboxOptions();
        configureOptions?.Invoke(options);

        if (options.EnableStatusCache)
        {
            // Nota: Esta implementação assume que IMftStatusClient já está registrado
            // (e.g., por AddMftMailboxSftp ou AddMftMailboxHttp).
            // Esta factory envolve o cliente existente com um camada de cache Redis.
            
            // Para preservar o cliente original, guardamos uma referência temporária
            var mftStatusClientDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMftStatusClient));
            
            if (mftStatusClientDescriptor is not null)
            {
                // Remover o registro anterior (será re-adicionado via factory)
                services.Remove(mftStatusClientDescriptor);
                
                // Registrar o cliente com decorador de cache
                _ = services.AddSingleton<IMftStatusClient>(sp =>
                {
                    // Resolver o cliente original através da factory anterior
                    var factory = ActivatorUtilities.CreateInstance(sp, mftStatusClientDescriptor.ImplementationType!);
                    var innerClient = (IMftStatusClient)factory;

                    return new RedisCachedStatusClient(
                        innerClient,
                        sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>(),
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
}
