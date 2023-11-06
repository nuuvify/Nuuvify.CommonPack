using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{
    public static class HttpWebRequestExtensionMethod
    {

        public static HttpWebRequest CustomRequestHeader(this HttpWebRequest request,
            IDictionary<string, object> header)
        {

            if (header is null || header.Count == 0)
            {
                header = new Dictionary<string, object>
                {
                    { Constants.CorrelationHeader, Guid.NewGuid() }
                };
            }

            var keyHeader = string.Empty;
            try
            {

                foreach (var item in header)
                {
                    keyHeader = item.Key;

                    if (string.IsNullOrWhiteSpace(request.Headers.Get(item.Key)))
                    {
                        request.Headers.Add(item.Key, item.Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("An item with the same key has already been added"))
                {
                    Debug.WriteLine("Chave ja existe: {key}", keyHeader);
                }
            }



            return request;
        }


    }
}
