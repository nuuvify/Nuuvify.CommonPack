namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

public class HttpMessageHandler : System.Net.Http.HttpMessageHandler
{

    public virtual HttpResponseMessage Send(HttpRequestMessage request)
    {
        throw new NotImplementedException("Esse conteudo não sera retornado, pois esse metodo esta sendo mocado");

    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken cancellationToken)
    {

        return Task.FromResult(Send(request));
    }
}
