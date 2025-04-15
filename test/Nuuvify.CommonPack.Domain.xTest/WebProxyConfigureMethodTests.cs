using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest;

public class WebProxyConfigureMethodTests
{

    [Fact]
    public void RetornaWebProxyComParametrosInformados()
    {
        var meudominio = "meudominio.com";
        var uri = new Uri($"https://lalala.{meudominio}");

        var webProxy = new WebProxyConfigureMethod(uri.AbsoluteUri, new string[] { meudominio });
        var proxyConfigured = webProxy.GetProxyWithVariable();

        Assert.NotNull(proxyConfigured);
        Assert.True(proxyConfigured.IsBypassed(uri), $"Url: {uri.AbsoluteUri} é bypass");

    }

}
