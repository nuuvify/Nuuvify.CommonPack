using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis.Services;

/// <summary>
/// Implementação de <see cref="IMftStatusClient"/> com cache distribuído via Redis.
/// </summary>
/// <remarks>
/// Decora outro <see cref="IMftStatusClient"/> (SFTP ou HTTP), adicionando cache em Redis.
/// Padrão: Verificar cache primeiro (Redis), fallback para cliente original se miss,
/// e cachear resultado para futuras consultas.
/// 
/// Reduz carga em servidores SFTP/HTTP remotos, mas pode retornar dados até
/// <c>RedisMftMailboxOptions.StatusCacheTtl</c> (padrão 5min) stale.
///
/// Registrado via <c>RedisMftMailboxSetup.AddMftMailboxRedis</c>.
/// </remarks>
public sealed class RedisCachedStatusClient : IMftStatusClient
{
    private readonly IMftStatusClient _innerClient;
    private readonly IDatabase _redis;
    private readonly RedisMftMailboxOptions _options;
    private readonly RedisStatusSerializer _serializer;
    private readonly ILogger<RedisCachedStatusClient> _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="RedisCachedStatusClient"/>.
    /// </summary>
    /// <param name="innerClient">Cliente de status original (SFTP ou HTTP).</param>
    /// <param name="connectionMultiplexer">Instância do <see cref="IConnectionMultiplexer"/> registrada no DI.</param>
    /// <param name="options">Opções de configuração Redis.</param>
    /// <param name="serializer">Serializador para <see cref="TransferStatus"/>.</param>
    /// <param name="logger">Logger para diagnóstico.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançado se qualquer parâmetro for <see langword="null"/>.
    /// </exception>
    public RedisCachedStatusClient(
        IMftStatusClient innerClient,
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisMftMailboxOptions> options,
        RedisStatusSerializer serializer,
        ILogger<RedisCachedStatusClient> logger)
    {
        if (innerClient == null)
            throw new ArgumentNullException(nameof(innerClient));
        if (connectionMultiplexer == null)
            throw new ArgumentNullException(nameof(connectionMultiplexer));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (serializer == null)
            throw new ArgumentNullException(nameof(serializer));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        _innerClient = innerClient;
        _redis = connectionMultiplexer.GetDatabase();
        _options = options.Value;
        _serializer = serializer;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o status de uma transferência, consultando Redis antes da fonte original.
    /// </summary>
    /// <param name="integrationKey">Chave de integração da transferência.</param>
    /// <param name="itemId">Identificador do item.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    /// <see cref="TransferStatus"/> com estado atual (do cache ou fonte), 
    /// ou <see langword="null"/> se não encontrado.
    /// </returns>
    /// <remarks>
    /// Estratégia:
    /// 1. Verificar cache Redis com chave <c>{prefix}:{integrationKey}:{itemId}</c>.
    /// 2. Se hit: retornar imediatamente (reduz latência).
    /// 3. Se miss: consultar cliente original.
    /// 4. Se resultado não-nulo: cachear com TTL configurado.
    /// 5. Retornar resultado (hit ou miss da fonte).
    /// </remarks>
    public async Task<TransferStatus?> GetStatusAsync(
        string integrationKey,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(integrationKey))
            throw new ArgumentException("Chave de integração não pode ser nula ou vazia.", nameof(integrationKey));
        if (string.IsNullOrWhiteSpace(itemId))
            throw new ArgumentException("ID do item não pode ser nulo ou vazio.", nameof(itemId));

        cancellationToken.ThrowIfCancellationRequested();

        var cacheKey = BuildCacheKey(integrationKey, itemId);

        try
        {
            // Tentar cache primeiro
            var cached = await GetFromCacheAsync(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug(
                    "Cache hit para status: {IntegrationKey}:{ItemId}",
                    integrationKey,
                    itemId);
                return cached;
            }

            _logger.LogDebug(
                "Cache miss para status: {IntegrationKey}:{ItemId}, consultando fonte original",
                integrationKey,
                itemId);

            // Fallback: consultar cliente original
            var status = await _innerClient.GetStatusAsync(integrationKey, itemId, cancellationToken);

            // Cachear resultado (mesmo se null, com TTL reduzido)
            if (status != null)
            {
                await SetCacheAsync(cacheKey, status, _options.StatusCacheTtl, cancellationToken);
            }
            else
            {
                // Cache negativo com TTL curto (1 min) para evitar consultas repetidas a "não encontrado"
                await SetNegativeCacheAsync(cacheKey, TimeSpan.FromMinutes(1), cancellationToken);
            }

            return status;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Erro de conexão Redis ao consultar status para {IntegrationKey}:{ItemId}, " +
                "consultando fonte original como fallback",
                integrationKey,
                itemId);

            // Fallback para cliente original se Redis falhar
            return await _innerClient.GetStatusAsync(integrationKey, itemId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao consultar status para {IntegrationKey}:{ItemId}",
                integrationKey,
                itemId);
            throw;
        }
    }

    /// <summary>
    /// Obtém status do cache Redis.
    /// </summary>
    private async Task<TransferStatus?> GetFromCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _redis.StringGetAsync(cacheKey);
            if (!cached.HasValue)
                return null;

            // Verificar se é cache negativo
            if (cached == "null")
                return null;

            return _serializer.Deserialize(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao desserializar cache de status para {CacheKey}", cacheKey);
            return null;
        }
    }

    /// <summary>
    /// Armazena status no cache Redis com TTL.
    /// </summary>
    private async Task SetCacheAsync(
        string cacheKey,
        TransferStatus status,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        try
        {
            var serialized = _serializer.Serialize(status);
            await _redis.StringSetAsync(cacheKey, serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao cachear status para {CacheKey}", cacheKey);
            // Não relançar: cache é otimização, falha não deve quebrar fluxo
        }
    }

    /// <summary>
    /// Cachear resultado negativo ("não encontrado") com TTL reduzido.
    /// </summary>
    private async Task SetNegativeCacheAsync(
        string cacheKey,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        try
        {
            await _redis.StringSetAsync(cacheKey, "null", ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao cachear resultado negativo para {CacheKey}", cacheKey);
        }
    }

    /// <summary>
    /// Constrói chave de cache Redis.
    /// </summary>
    private string BuildCacheKey(string integrationKey, string itemId) =>
        $"{_options.StatusCacheKeyPrefix}{integrationKey}:{itemId}";
}
