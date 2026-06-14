using System.Globalization;
using System.Text;

namespace System.Collections.Generic;

public static class CustomDictionaryExtensions
{
    /// <summary>
    /// QueryString não pode exceder 128 caracteres
    /// </summary>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static string ToQueryString(this IDictionary<string, string> dic)
    {
        var query = new StringBuilder(capacity: 128);

        if (query.Capacity > 128)
        {
            query.Capacity = 128;
        }

        if (query.Length == 0 || query[0] != '?')
        {
            _ = query.Insert(0, '?');
        }

        for (int i = 0; i < dic.Count; i++)
        {
            _ = query.Append(CultureInfo.InvariantCulture, $"{dic.ElementAtOrDefault(i).Key}={dic.ElementAtOrDefault(i).Value}&");

        }

        return QueryString(query.ToString());

    }

    private static string QueryString(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value[0] != '?')
        {
            throw new ArgumentException("The leading '?' must be included for a non-empty query.", nameof(value));
        }

        if (value.EndsWith('&') || value.EndsWith('?'))
            value = value[..^1];

        return value;
    }

}
