using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;


namespace Nuuvify.CommonPack.StandardHttpClient.Helpers
{
    internal class NotificationRConverter : JsonConverter<NotificationR>
    {

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(NotificationR) == typeToConvert;
        }

        public override NotificationR Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
        {

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }


            var notification = typeof(NotificationR);
            var result = Activator.CreateInstance(notification, nonPublic: true);
            PropertyInfo propertyInfo;
            object itemValue;
            string propertyName;


            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return (NotificationR)result;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                propertyName = reader.GetString() ?? "";

                propertyInfo = notification.GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

                if (propertyInfo != null)
                {
                    itemValue = reader.ConvertJsonTypeCustom(propertyInfo.PropertyType);
                    propertyInfo.SetValue(result, itemValue);
                }
            }

            return (NotificationR)result;

        }

        public override void Write(
            Utf8JsonWriter writer,
            NotificationR value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }


    }
  
}