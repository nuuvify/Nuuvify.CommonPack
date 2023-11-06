using System;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Nuuvify.CommonPack.Extensions.JsonConverter
{
    public class JsonDateTimeOffsetToInferredTypesConverter : JsonConverter<DateTimeOffset>
    {

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(DateTimeOffset) == typeToConvert;
        }

        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {

            DateTimeOffset.TryParse(reader.GetString(), out DateTimeOffset result);
            return result;
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset value,
            JsonSerializerOptions options)
        {
            DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset result);
            writer.WriteStringValue(result);

        }

    }
}