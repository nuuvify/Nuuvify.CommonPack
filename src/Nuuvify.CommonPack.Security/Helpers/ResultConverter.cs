using System;
using System.Linq;
using System.Reflection;
using Nuuvify.CommonPack.Security.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nuuvify.CommonPack.Security.Helpers
{
    internal class ResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CredentialToken);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var myClass = typeof(CredentialToken);
            var result = Activator.CreateInstance(myClass, nonPublic: true);
            PropertyInfo propertyInfo;
            object itemValue;

            JObject jo = JObject.Load(reader);


            foreach (var item in jo)
            {

                propertyInfo = myClass.GetProperties()
                    .FirstOrDefault(x => x.Name.ToUpperInvariant() == item.Key.ToUpperInvariant());
                
                if (propertyInfo != null)
                {
                    itemValue = Convert.ChangeType(item.Value, propertyInfo.PropertyType);

                    propertyInfo.SetValue(result, itemValue);
                }

            }


            return result;

        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}