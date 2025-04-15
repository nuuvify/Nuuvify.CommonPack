using Polly;
using Polly.Extensions.Http;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly;

public static class HttpPolicyBuilders
{
    public static PolicyBuilder<HttpResponseMessage> GetBaseBuilder()
    {
        return HttpPolicyExtensions.HandleTransientHttpError();
    }
}
