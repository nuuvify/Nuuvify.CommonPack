using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public partial class StandardHttpClientService
{

    ///<inheritdoc/>
    public async Task<HttpStandardStreamReturn> GetStream(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        using var message = new HttpRequestMessage(HttpMethod.Get, url)
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);

        return await StandardStreamSendAsync(url, message, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpStandardReturn> Get(
        string urlRoute,
        CancellationToken cancellationToken = default)
    {
        var url = $"{urlRoute}{_queryString.ToQueryString()}";

        using var message = new HttpRequestMessage(HttpMethod.Get, url)
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);

        return await StandardSendAsync(url, message, cancellationToken).ConfigureAwait(false);
    }

}

