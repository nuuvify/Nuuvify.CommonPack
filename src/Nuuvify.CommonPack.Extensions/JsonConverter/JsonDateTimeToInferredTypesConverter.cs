using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nuuvify.CommonPack.Extensions.JsonConverter;

public class JsonDateTimeToInferredTypesConverter : JsonConverter<DateTime>
{

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(DateTime) == typeToConvert;
    }

    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {

        _ = DateTime.TryParse(reader.GetString(), out DateTime result);
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        _ = DateTime.TryParse(value.ToString(), out DateTime result);
        writer.WriteStringValue(result);

    }

}
