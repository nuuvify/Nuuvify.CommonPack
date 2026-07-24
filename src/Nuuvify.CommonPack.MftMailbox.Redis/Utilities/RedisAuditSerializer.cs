using System.Globalization;
using System.Text.Json;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis.Utilities;

/// <summary>
/// Serializador para <see cref="TransferAuditEntry"/> em Redis Streams.
/// </summary>
/// <remarks>
/// Converte entidades de auditoria para NameValueEntry[] compatível com XADD do Redis.
/// </remarks>
public sealed class RedisAuditSerializer
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializa <see cref="TransferAuditEntry"/> para array de name-value pairs do Redis.
    /// </summary>
    public NameValueEntry[] Serialize(TransferAuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new[]
        {
            new NameValueEntry("timestamp", entry.TimestampUtc.UtcTicks.ToString(CultureInfo.InvariantCulture)),
            new NameValueEntry("integrationKey", entry.IntegrationKey),
            new NameValueEntry("correlationId", entry.CorrelationId),
            new NameValueEntry("itemId", entry.ItemId),
            new NameValueEntry("fileName", entry.FileName),
            new NameValueEntry("protocol", entry.Protocol.ToString()),
            new NameValueEntry("state", entry.State.ToString()),
            new NameValueEntry("message", entry.Message ?? string.Empty),
            new NameValueEntry("metadata", SerializeMetadata(entry.Metadata))
        };
    }

    /// <summary>
    /// Desserializa entry de Redis Stream para <see cref="TransferAuditEntry"/>.
    /// </summary>
    public TransferAuditEntry? Deserialize(StreamEntry streamEntry)
    {
        try
        {
            var nameValues = streamEntry.Values.ToDictionary(x => x.Name.ToString(), x => (string?)x.Value.ToString(), StringComparer.Ordinal);

            return new TransferAuditEntry
            {
                TimestampUtc = new DateTimeOffset(
                    long.Parse(nameValues.GetValueOrDefault("timestamp", "0") ?? "0", CultureInfo.InvariantCulture),
                    TimeSpan.Zero),
                IntegrationKey = nameValues.GetValueOrDefault("integrationKey", string.Empty) ?? string.Empty,
                CorrelationId = nameValues.GetValueOrDefault("correlationId", string.Empty) ?? string.Empty,
                ItemId = nameValues.GetValueOrDefault("itemId", string.Empty) ?? string.Empty,
                FileName = nameValues.GetValueOrDefault("fileName", string.Empty) ?? string.Empty,
                Protocol = Enum.Parse<MftProtocol>(nameValues.GetValueOrDefault("protocol", "Sftp") ?? "Sftp"),
                State = Enum.Parse<TransferState>(nameValues.GetValueOrDefault("state", "Pending") ?? "Pending"),
                Message = nameValues.GetValueOrDefault("message", null),
                Metadata = DeserializeMetadata(nameValues.GetValueOrDefault("metadata", "{}") ?? "{}")
            };
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            // Falha ao desserializar stream entry malformado; retorna null para ignorar
            return null;
        }
    }

    private static string SerializeMetadata(IDictionary<string, string> metadata)
    {
        if (metadata == null || metadata.Count == 0)
            return "{}";

        return JsonSerializer.Serialize(metadata, s_jsonOptions);
    }

    private static IDictionary<string, string> DeserializeMetadata(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, s_jsonOptions)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return new Dictionary<string, string>(result, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            // JSON inválido; retorna dicionário vazio
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
