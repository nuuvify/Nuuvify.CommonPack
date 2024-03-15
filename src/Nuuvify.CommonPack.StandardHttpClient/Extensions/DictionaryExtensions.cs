using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nuuvify.CommonPack.StandardHttpClient.Extensions
{
    public static class CustomDictionaryExtensions
    {

        private static StringBuilder _query;

        public static string ToQueryString(this IDictionary<string, string> dic)
        {
            _query = new StringBuilder();
            if (_query.Capacity > 128)
            {
                _query.Capacity = 128;
            }


            if (_query.Length == 0 || _query[0] != '?')
            {
                _query.Insert(0, '?');
            }

            for (int i = 0; i < dic.Count; i++)
            {
                _query.Append($"{dic.ElementAtOrDefault(i).Key}={dic.ElementAtOrDefault(i).Value},");

            }

            return QueryString(_query.ToString());


        }

        private static string QueryString(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && value[0] != '?')
            {
                throw new ArgumentException("The leading '?' must be included for a non-empty query.", nameof(value));
            }

            if (value.EndsWith(','))
                value = value[..^1];

            return value;
        }

    }

}