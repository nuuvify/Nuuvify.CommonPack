namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

public class DelegatingHandlerStub : DelegatingHandler
{
    private readonly HttpResponseMessage _fakeResponse;

    public DelegatingHandlerStub(HttpResponseMessage responseMessage)
    {
        _fakeResponse = responseMessage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_fakeResponse);
    }
}
