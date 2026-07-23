using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis.Services;

/// <summary>
/// Implementação de <see cref="ITransferAuditSink"/> usando Redis Streams como buffer de auditoria.
/// </summary>
/// <remarks>
/// Grava cada operação MFT (envio, recepção, ACK/NACK) em uma stream Redis que pode ser
/// consumida por aplicações externas (logging, telemetria, banco de dados).
///
/// Padrão: Fire-and-forget assíncrono (não bloqueia fluxo de transferência).
/// Consumidores externos leem da stream com <c>XREAD</c> ou grupos de consumo.
///
/// Registrado via <c>RedisMftMailboxSetup.AddMftMailboxRedis</c> quando
/// <c>RedisMftMailboxOptions.EnableAuditStream</c> for <see langword="true"/>.
/// </remarks>
public sealed class RedisAuditStreamSink : ITransferAuditSink
{
    private readonly IDatabase _redis;
    private readonly RedisMftMailboxOptions _options;
    private readonly RedisAuditSerializer _serializer;
    private readonly ILogger<RedisAuditStreamSink> _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="RedisAuditStreamSink"/>.
    /// </summary>
    /// <param name="connectionMultiplexer">Instância do <see cref="IConnectionMultiplexer"/> registrada no DI.</param>
    /// <param name="options">Opções de configuração Redis.</param>
    /// <param name="serializer">Serializador para <see cref="TransferAuditEntry"/>.</param>
    /// <param name="logger">Logger para diagnóstico.</param>
    /// <exception cref="ArgumentNullException">
    /// Lançado se qualquer parâmetro for <see langword="null"/>.
    /// </exception>
    public RedisAuditStreamSink(
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisMftMailboxOptions> options,
        RedisAuditSerializer serializer,
        ILogger<RedisAuditStreamSink> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(logger);

        _redis = connectionMultiplexer.GetDatabase();
        _options = options.Value;
        _serializer = serializer;
        _logger = logger;
    }

    /// <summary>
    /// Grava uma entrada de auditoria na stream Redis de forma assíncrona e não-bloqueante.
    /// </summary>
    /// <param name="entry">Dados da operação a ser auditada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <remarks>
    /// Operação é fire-and-forget: não aguarda conclusão. Se falhar, apenas loga
    /// warning e continua. Falhas não interrompem o fluxo de transferência MFT.
    ///
    /// Stream é trimada automaticamente para <c>RedisMftMailboxOptions.AuditStreamMaxLength</c>
    /// (padrão 100k entradas) para evitar crescimento indefinido.
    /// </remarks>
    public async Task WriteAsync(TransferAuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var nameValueEntries = _serializer.Serialize(entry);

            // Adicionar à stream Redis
            var streamId = await _redis.StreamAddAsync(
                _options.AuditStreamName,
                nameValueEntries);

            _logger.LogDebug(
                "Entrada de auditoria adicionada à stream: {StreamId}, " +
                "IntegrationKey={IntegrationKey}, ItemId={ItemId}, State={State}",
                streamId,
                entry.IntegrationKey,
                entry.ItemId,
                entry.State);

            // Trimagem assíncrona: manter apenas últimas N entradas
            // Executada em background, não bloqueia
            _ = _redis.StreamTrimAsync(
                _options.AuditStreamName,
                _options.AuditStreamMaxLength);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Erro de conexão Redis ao gravar auditoria para IntegrationKey={IntegrationKey}",
                entry.IntegrationKey);
            // Não relançar: auditoria é auxiliar, falha não deve quebrar fluxo
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Erro ao gravar auditoria para IntegrationKey={IntegrationKey}, ItemId={ItemId}",
                entry.IntegrationKey,
                entry.ItemId);
            // Não relançar: auditoria é fire-and-forget
        }
    }
}
