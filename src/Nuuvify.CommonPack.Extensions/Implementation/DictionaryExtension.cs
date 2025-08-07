using System.Collections.Generic;

namespace Nuuvify.CommonPack.Extensions.Implementation;

public static class DictionaryExtension
{

    public static void AddForce(this IDictionary<string, object> dictionary, string key, object value)
    {

        if (dictionary.TryGetValue(key, out object valueTest))
        {
            _ = dictionary.Remove(key);
        }
        dictionary.Add(key, value);

    }
}
