using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis.Services;

/// <summary>
/// Armazenamento de idempotência distribuído via Redis com TTL automático.
/// </summary>
/// <remarks>
/// Garante que múltiplas instâncias não processem o mesmo arquivo mesmo que rodando em paralelo.
/// Usa SET NX (Set If Not eXists) para atomicidade e TTL para auto-cleanup.
/// 
/// Ciclo de vida de uma chave:
/// <list type="bullet">
/// <item><c>TryStartAsync</c>: Tenta criar chave com valor "started" e TTL. Retorna <see langword="true"/> se criada.</item>
/// <item><c>MarkCompletedAsync</c>: Atualiza valor para "completed", preservando TTL restante.</item>
/// <item><c>MarkFailedAsync</c>: Atualiza valor para "failed:{reason}", preservando TTL restante.</item>
/// <item>Expiração automática: Após TTL, chave é removida do Redis.</item>
/// </list>
///
/// Registrada como Singleton via <c>RedisMftMailboxSetup.AddMftMailboxRedis</c>.
/// </remarks>
public sealed class RedisMftIdempotencyStore : IMftIdempotencyStore
{
    private readonly IDatabase _redis;
    private readonly RedisMftMailboxOptions _options;
    private readonly ILogger<RedisMftIdempotencyStore> _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="RedisMftIdempotencyStore"/>.
    /// </summary>
    /// <param name="connectionMultiplexer">
    /// Instância do <see cref="IConnectionMultiplexer"/> já registrada no DI.
    /// Deve estar conectada e operacional.
    /// </param>
    /// <param name="options">Opções de configuração Redis do MftMailbox.</param>
    /// <param name="logger">Logger para diagnóstico e troubleshooting.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançado se <paramref name="connectionMultiplexer"/>, <paramref name="options"/> ou <paramref name="logger"/> forem <see langword="null"/>.
    /// </exception>
    public RedisMftIdempotencyStore(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisMftMailboxOptions> options,
        ILogger<RedisMftIdempotencyStore> logger)
    {
        if (connectionMultiplexer == null)
            throw new ArgumentNullException(nameof(connectionMultiplexer));
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        _redis = connectionMultiplexer.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Tenta registrar a chave como iniciada atomicamente no Redis.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta gerada por <c>IdempotencyKeyBuilder</c>.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>
    /// <see langword="true"/> se a chave foi inserida pela primeira vez (processamento pode prosseguir);
    /// <see langword="false"/> se já existia (item já processado ou em andamento).
    /// </returns>
    /// <remarks>
    /// Usa <c>SET NX EX</c> para garantir atomicidade entre múltiplas instâncias.
    /// Se a chave já existe, retorna <see langword="false"/> e a operação é descartada.
    /// </remarks>
    /// <exception cref="RedisConnectionException">
    /// Lançado se Redis está indisponível ou ocorre erro de rede durante a operação.
    /// </exception>
    public async Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var redisKey = BuildRedisKey(idempotencyKey);

            // SET NX EX: Set if Not eXists, with EXpiry
            var started = await _redis.StringSetAsync(
                redisKey,
                "started",
                _options.IdempotencyTtl,
                When.NotExists);

            if (!started)
            {
                _logger.LogDebug(
                    "Chave de idempotência já existe (reprocessamento evitado): {IdempotencyKey}",
                    idempotencyKey);
            }
            else
            {
                _logger.LogDebug(
                    "Chave de idempotência registrada como iniciada: {IdempotencyKey} (TTL: {Ttl}s)",
                    idempotencyKey,
                    _options.IdempotencyTtl.TotalSeconds);
            }

            return started;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(
                ex,
                "Erro de conexão Redis em TryStartAsync para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao tentar iniciar idempotência para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }

    /// <summary>
    /// Marca a chave como concluída, preservando o TTL restante.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <remarks>
    /// Consulta o TTL restante da chave e mantém o mesmo prazo ao atualizar o valor.
    /// Se a chave expirou, usa TTL padrão.
    /// </remarks>
    /// <exception cref="RedisConnectionException">
    /// Lançado se Redis está indisponível ou ocorre erro de rede.
    /// </exception>
    public async Task MarkCompletedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var redisKey = BuildRedisKey(idempotencyKey);

            // Obter TTL restante
            var ttlTimeSpan = await _redis.KeyTimeToLiveAsync(redisKey);
            var ttl = (ttlTimeSpan > TimeSpan.Zero) ? ttlTimeSpan : _options.IdempotencyTtl;

            // Atualizar valor mantendo TTL
            await _redis.StringSetAsync(redisKey, "completed", ttl);

            _logger.LogDebug(
                "Chave de idempotência marcada como concluída: {IdempotencyKey}",
                idempotencyKey);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(
                ex,
                "Erro de conexão Redis em MarkCompletedAsync para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao marcar idempotência como concluída para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }

    /// <summary>
    /// Marca a chave como falha, preservando o motivo e o TTL restante.
    /// </summary>
    /// <param name="idempotencyKey">Chave composta da transferência.</param>
    /// <param name="reason">Descrição do motivo da falha (não pode ser nula ou vazia).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <remarks>
    /// Armazena o valor como <c>"failed:{reason}"</c> para diagnóstico posterior.
    /// TTL restante é preservado. Se a chave expirou, usa TTL padrão.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Lançado se <paramref name="reason"/> for nula ou vazia.
    /// </exception>
    /// <exception cref="RedisConnectionException">
    /// Lançado se Redis está indisponível ou ocorre erro de rede.
    /// </exception>
    public async Task MarkFailedAsync(string idempotencyKey, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Chave de idempotência não pode ser nula ou vazia.", nameof(idempotencyKey));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Motivo da falha não pode ser nulo ou vazio.", nameof(reason));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var redisKey = BuildRedisKey(idempotencyKey);
            var ttlTimeSpan = await _redis.KeyTimeToLiveAsync(redisKey);
            var ttl = (ttlTimeSpan > TimeSpan.Zero) ? ttlTimeSpan : _options.IdempotencyTtl;

            // Armazenar falha com motivo
            await _redis.StringSetAsync(redisKey, $"failed:{reason}", ttl);

            _logger.LogDebug(
                "Chave de idempotência marcada como falha: {IdempotencyKey}, Motivo: {Reason}",
                idempotencyKey,
                reason);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(
                ex,
                "Erro de conexão Redis em MarkFailedAsync para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao marcar idempotência como falha para chave {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }

    /// <summary>
    /// Constrói a chave Redis final com prefixo configurado.
    /// </summary>
    private string BuildRedisKey(string idempotencyKey) =>
        $"{_options.IdempotencyKeyPrefix}{idempotencyKey}";
}
