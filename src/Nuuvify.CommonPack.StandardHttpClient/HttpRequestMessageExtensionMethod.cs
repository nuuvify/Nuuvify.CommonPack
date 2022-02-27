using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.StandardHttpClient
{
    public static class HttpRequestMessageExtensionMethod
    {

        public static HttpRequestMessage CustomRequestHeader(this HttpRequestMessage request,
            IDictionary<string, object> header)
        {

            if (header is null || header.Count == 0)
            {
                header = new Dictionary<string, object>
                {
                    { Constants.CorrelationHeader, Guid.NewGuid() }
                };
            }
            else
            {
                header.Remove("bearer");
                header.Remove("Authorization");
            }

            var keyHeader = string.Empty;
            try
            {

                foreach (var item in header)
                {
                    keyHeader = item.Key;

                    if (!request.Headers.TryGetValues(item.Key, out IEnumerable<string> values))
                    {
                        request.Headers.TryAddWithoutValidation(item.Key, item.Value.ToString());
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

        public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage request,
            IDictionary<string, object> header)
        {

            if (header.NotNullOrZero())
            {
                var auth = header.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Key));

                if (auth.Key.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (auth.Value.ToString().StartsWith("bearer", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return AddAuthorizationHeader(request, "bearer", auth.Value.ToString());
                    }
                    else if (auth.Value.ToString().StartsWith("basic", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return AddAuthorizationHeader(request, "basic", auth.Value.ToString());
                    }
                    else
                    {
                        throw new HttpRequestException("This lib is prepared only for Bearer or Basic scheme");
                    }
                }
            }

            return request;
        }

        public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage request,
            string scheme = "bearer",
            string tokenOrPassword = null)
        {

            if (scheme.Equals("bearer", StringComparison.InvariantCultureIgnoreCase))
            {
                if (request.Headers.Authorization != null)
                {
                    request.Headers.Remove("Authorization");
                }

                request.Headers.Authorization =
                    new AuthenticationHeaderValue(scheme, tokenOrPassword?.Replace("bearer", "").TrimStart());
            }
            else if (scheme.Equals("basic", StringComparison.InvariantCultureIgnoreCase))
            {
                if (request.Headers.Authorization != null)
                {
                    request.Headers.Remove("Authorization");
                }

                var token = tokenOrPassword?.Replace("basic", "").TrimStart();
                var userPassword = Encoding.ASCII.GetBytes(token);
                var base64 = Convert.ToBase64String(userPassword);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue(scheme, base64);
            }


            return request;
        }
    }
}
