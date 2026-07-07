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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializa <see cref="TransferAuditEntry"/> para array de name-value pairs do Redis.
    /// </summary>
    public NameValueEntry[] Serialize(TransferAuditEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        return new[]
        {
            new NameValueEntry("timestamp", entry.TimestampUtc.UtcTicks.ToString()),
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
            var nameValues = streamEntry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

            return new TransferAuditEntry
            {
                TimestampUtc = new DateTimeOffset(long.Parse(nameValues.GetValueOrDefault("timestamp", "0")), TimeSpan.Zero),
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
        catch
        {
            return null;
        }
    }

    private static string SerializeMetadata(IDictionary<string, string> metadata)
    {
        if (metadata == null || metadata.Count == 0)
            return "{}";

        return JsonSerializer.Serialize(metadata, JsonOptions);
    }

    private static IDictionary<string, string> DeserializeMetadata(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return new Dictionary<string, string>(result, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
