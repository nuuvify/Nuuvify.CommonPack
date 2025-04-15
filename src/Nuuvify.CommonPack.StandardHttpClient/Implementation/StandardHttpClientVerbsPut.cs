using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public partial class StandardHttpClient
{

    public async Task<HttpStandardReturn> Put(string urlRoute, object messageBody, CancellationToken cancellationToken = default)
    {

        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        var message = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(messageBody),
                Encoding.UTF8, "application/json"
            )
        }
        .CustomRequestHeader(_headerStandard)
        .AddAuthorizationHeader(_headerAuthorization);

        return await StandardSendAsync(url, message, cancellationToken);

    }

    public async Task<HttpStandardReturn> Put(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        var message = new HttpRequestMessage(HttpMethod.Put, url);

        IEnumerable<KeyValuePair<string, string>> enumerable = _formParameter;

        var content = new FormUrlEncodedContent(enumerable);

        message.Content = content;
        _ = message.CustomRequestHeader(_headerStandard)
               .AddAuthorizationHeader(_headerAuthorization);

        return await StandardSendAsync(url, message, cancellationToken);

    }

    public async Task<HttpStandardStreamReturn> PutStream(
        string urlRoute,
        MultipartFormDataContent messageBody,
        string mediaType = "multipart/form-data",
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        var message = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = messageBody
        }
        .CustomRequestHeader(_headerStandard)
        .AddAuthorizationHeader(_headerAuthorization);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

        return await StandardStreamSendAsync(url, message, cancellationToken);
    }

}

