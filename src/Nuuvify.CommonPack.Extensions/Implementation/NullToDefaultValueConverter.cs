
namespace System.Text.Json.Serialization;

public class NullToDefaultValueConverter<T> : JsonConverter<T> where T : struct, IConvertible
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        if (reader.TokenType.ToString().ToLower() != "number" || reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (typeof(T) == typeof(int))
        {
            return (T)(object)reader.GetInt32();
        }
        else if (typeof(T) == typeof(double))
        {
            return (T)(object)reader.GetDouble();
        }
        else if (typeof(T) == typeof(float))
        {
            return (T)(object)reader.GetSingle();
        }
        else if (typeof(T) == typeof(long))
        {
            return (T)(object)reader.GetInt64();
        }
        else if (typeof(T) == typeof(decimal))
        {
            return (T)(object)reader.GetDecimal();
        }
        else
        {
            throw new JsonException($"Tipo {typeof(T)} não é suportado.");
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (typeof(T) == typeof(int))
        {
            writer.WriteNumberValue((int)(object)value);
        }
        else if (typeof(T) == typeof(double))
        {
            writer.WriteNumberValue((double)(object)value);
        }
        else if (typeof(T) == typeof(float))
        {
            writer.WriteNumberValue((float)(object)value);
        }
        else if (typeof(T) == typeof(long))
        {
            writer.WriteNumberValue((long)(object)value);
        }
        else if (typeof(T) == typeof(decimal))
        {
            writer.WriteNumberValue((decimal)(object)value);
        }
        else if (typeof(T) == typeof(string))
        {
            writer.WriteStringValue((string)(object)value);
        }
        else
        {
            throw new JsonException($"Tipo {typeof(T)} não é suportado.");
        }
    }
}

