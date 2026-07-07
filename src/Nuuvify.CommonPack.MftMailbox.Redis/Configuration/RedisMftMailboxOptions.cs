namespace Nuuvify.CommonPack.MftMailbox.Redis.Configuration;

/// <summary>
/// Opções de configuração para integração Redis com MFT Mailbox.
/// </summary>
/// <remarks>
/// Registrada via <c>AddMftMailboxRedis</c> no container de DI.
/// <code>
/// services.AddMftMailboxRedis(opts =>
/// {
///     opts.IdempotencyKeyPrefix = "mft:";
///     opts.IdempotencyTtl = TimeSpan.FromHours(24);
///     opts.StatusCacheTtl = TimeSpan.FromMinutes(5);
///     opts.EnableAuditStream = true;
///     opts.AuditStreamName = "mft-audit";
/// });
/// </code>
/// </remarks>
public sealed class RedisMftMailboxOptions
{
    /// <summary>
    /// Prefixo para todas as chaves de idempotência no Redis.
    /// Padrão: <c>"mft:idempotency:"</c>.
    /// </summary>
    public string IdempotencyKeyPrefix { get; set; } = "mft:idempotency:";

    /// <summary>
    /// Tempo de vida (TTL) das chaves de idempotência no Redis.
    /// Padrão: <c>TimeSpan.FromHours(24)</c> (24 horas).
    /// </summary>
    /// <remarks>
    /// Chaves expiram automaticamente, evitando crescimento indefinido de dados no Redis.
    /// Configure baseado na sua SLA de reprocessamento (ex: duplicação detectada em até X horas).
    /// </remarks>
    public TimeSpan IdempotencyTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Prefixo para chaves de status de transferência no cache Redis.
    /// Padrão: <c>"mft:status:"</c>.
    /// </summary>
    public string StatusCacheKeyPrefix { get; set; } = "mft:status:";

    /// <summary>
    /// Tempo de vida (TTL) do cache de status no Redis.
    /// Padrão: <c>TimeSpan.FromMinutes(5)</c> (5 minutos).
    /// </summary>
    /// <remarks>
    /// Dados de status cacheados por esse período. Após expiração, a próxima consulta
    /// vai para a fonte (servidor SFTP/HTTP). Configure com base na volatilidade do status
    /// e tolerância a staleness.
    /// </remarks>
    public TimeSpan StatusCacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Habilita auditoria via Redis Streams.
    /// Padrão: <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Quando <see langword="true"/>, cada operação MFT (envio, recepção, ACK/NACK)
    /// é gravada em uma stream Redis que pode ser consumida por aplicações externas.
    /// </remarks>
    public bool EnableAuditStream { get; set; } = true;

    /// <summary>
    /// Nome da stream Redis para auditoria.
    /// Padrão: <c>"mft-audit"</c>.
    /// </summary>
    /// <remarks>
    /// Usado apenas se <see cref="EnableAuditStream"/> for <see langword="true"/>.
    /// Consumidores podem ler dessa stream com <c>XREAD</c> ou grupos de consumo.
    /// </remarks>
    public string AuditStreamName { get; set; } = "mft-audit";

    /// <summary>
    /// Tamanho máximo da stream Redis em número de entradas.
    /// Padrão: <c>100000</c> (100k entradas).
    /// </summary>
    /// <remarks>
    /// Redis usa XTRIM MAXLEN para manter a stream dentro desse limite.
    /// Configure baseado em volume esperado e retenção desejada.
    /// </remarks>
    public int AuditStreamMaxLength { get; set; } = 100_000;

    /// <summary>
    /// Habilita cache de status compartilhado entre instâncias.
    /// Padrão: <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// Quando <see langword="true"/>, consultas de status são cacheadas no Redis
    /// e reutilizadas por outras instâncias durante o TTL. Reduz carga em servidores
    /// SFTP/HTTP remotos, mas pode retornar dados até TTL stale.
    /// </remarks>
    public bool EnableStatusCache { get; set; } = true;

    /// <summary>
    /// Timeout para operações Redis.
    /// Padrão: <c>TimeSpan.FromSeconds(5)</c> (5 segundos).
    /// </summary>
    /// <remarks>
    /// Se Redis não responder dentro desse período, a operação falha com timeout.
    /// Configure maior se tiver latência de rede elevada.
    /// </remarks>
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
