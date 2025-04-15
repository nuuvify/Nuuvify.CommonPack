using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.StandardHttpClient;

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

        var keyHeader = string.Empty;
        try
        {

            foreach (var item in header)
            {
                keyHeader = item.Key;

                if (!request.Headers.TryGetValues(item.Key, out IEnumerable<string> values))
                {
                    _ = request.Headers.TryAddWithoutValidation(item.Key, item.Value.ToString());
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

    public static HttpClient CustomRequestHeader(this HttpClient request,
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

                if (!request.DefaultRequestHeaders.TryGetValues(item.Key, out IEnumerable<string> values))
                {
                    _ = request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value.ToString());
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

            if (auth.Key.ToString().StartsWith("bearer", StringComparison.InvariantCultureIgnoreCase) ||
                auth.Key.ToString().StartsWith("basic", StringComparison.InvariantCultureIgnoreCase))
            {
                return AddAuthorizationHeader(request, auth.Key, auth.Value.ToString());
            }
            else
            {
                throw new HttpRequestException("This lib is prepared only for Bearer or Basic scheme");
            }
        }

        return request;
    }

    public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage request,
        string scheme = null,
        string tokenOrPassword = null)
    {

        if (string.IsNullOrWhiteSpace(scheme)) return request;

        if (scheme.Equals("bearer", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Headers.Authorization != null)
            {
                _ = request.Headers.Remove("Authorization");
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue(scheme, tokenOrPassword);

        }
        else if (scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Headers.Authorization != null)
            {
                _ = request.Headers.Remove("Authorization");
            }

            var userPassword = Encoding.ASCII.GetBytes(tokenOrPassword);
            var base64 = Convert.ToBase64String(userPassword);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(scheme, base64);

        }

        return request;
    }
}

