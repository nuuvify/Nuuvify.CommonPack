using System;
using System.Text.Json;


namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class JsonTypesExtensions
    {


        public static object ConvertJsonTypeCustom(this ref Utf8JsonReader reader, Type propertyType)
        {
            object itemValue;


            reader.Read();
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                throw new JsonException($"Nao era esperado uma propriedade, e sim um valor do tipo: {propertyType.Name}");
            }

            object propertyValue = propertyType.Name.ToLowerInvariant() switch
            {
                "string" => reader.GetString(),
                "datetimeoffset" => reader.GetDateTimeOffset(),
                "datetime" => reader.GetDateTime(),
                "decimal" => reader.GetDecimal(),
                "double" => reader.GetDouble(),
                "float" => reader.GetSingle(),
                "byte" => reader.GetByte(),
                "short" => reader.GetInt16(),
                "int" => reader.GetInt32(),
                "long" => reader.GetInt64(),
                "guid" => reader.GetGuid(),
                "bool" => reader.GetBoolean(),
                "byte[]" => reader.GetBytesFromBase64(),
                _ => reader.GetComment(),
            };
            itemValue = Convert.ChangeType(propertyValue, propertyType);
            return itemValue;

        }

    }

}