using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public partial class StandardHttpClient
{

    ///<inheritdoc/>
    public async Task<HttpStandardStreamReturn> GetStream(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        var message = new HttpRequestMessage(HttpMethod.Get, url)
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);

        return await StandardStreamSendAsync(url, message, cancellationToken);
    }

    public async Task<HttpStandardReturn> Get(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        var message = new HttpRequestMessage(HttpMethod.Get, url)
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);

        return await StandardSendAsync(url, message, cancellationToken);
    }

}

