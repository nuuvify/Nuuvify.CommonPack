using Nuuvify.CommonPack.StandardHttpClient.Results;
using System.Net.Http.Headers;

namespace Nuuvify.CommonPack.StandardHttpClient;


public partial class StandardHttpClient
{


    public async Task<HttpStandardReturn> Delete(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";


        var message = new HttpRequestMessage(HttpMethod.Delete, url)
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


        return await StandardSendAsync(url, message, cancellationToken);

    }

    public async Task<HttpStandardStreamReturn> DeleteStream(
        string urlRoute,
        MultipartFormDataContent messageBody,
        string mediaType = "multipart/form-data",
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";


        var message = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = messageBody
        }
        .CustomRequestHeader(_headerStandard)
        .AddAuthorizationHeader(_headerAuthorization);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));



        return await StandardStreamSendAsync(url, message, cancellationToken);
    }


}

