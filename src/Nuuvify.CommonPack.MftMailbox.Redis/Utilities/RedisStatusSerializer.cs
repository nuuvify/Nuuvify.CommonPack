using System.Text.Json;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using StackExchange.Redis;

namespace Nuuvify.CommonPack.MftMailbox.Redis.Utilities;

/// <summary>
/// Serializador para <see cref="TransferStatus"/> em Redis.
/// </summary>
/// <remarks>
/// Usa JSON (System.Text.Json) para serialização/desserialização de objetos de status.
/// </remarks>
public sealed class RedisStatusSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializa <see cref="TransferStatus"/> para string JSON.
    /// </summary>
    public string Serialize(TransferStatus status)
    {
        if (status == null)
            throw new ArgumentNullException(nameof(status));

        return JsonSerializer.Serialize(status, JsonOptions);
    }

    /// <summary>
    /// Desserializa JSON de Redis para <see cref="TransferStatus"/>.
    /// </summary>
    public TransferStatus? Deserialize(RedisValue value)
    {
        if (!value.HasValue)
            return null;

        try
        {
            return JsonSerializer.Deserialize<TransferStatus>(value.ToString(), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
