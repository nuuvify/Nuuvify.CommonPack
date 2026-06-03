using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.Middleware.Handle;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class HandlingHeadersMiddlewareTests
{
    private static IOptions<RequestConfiguration> BuildOptions(string appName = "TestApp")
    {
        var config = new RequestConfiguration
        {
            AppName = appName,
            ApplicationVersion = "1.0.0",
            BuildNumber = "2024.01.01",
            Environment = "Test",
            HostName = "localhost"
        };
        return Options.Create(config);
    }

    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(HandlingHeadersMiddleware))]
    public async Task Invoke_SemCorrelationIdNoHeader_AdicionaCorrelationIdNaResposta()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var logger = NullLogger<HandlingHeadersMiddleware>.Instance;
        var middleware = new HandlingHeadersMiddleware(next, logger, BuildOptions());
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.True(context.Response.Headers.ContainsKey(Constants.CorrelationHeader));
    }

    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(HandlingHeadersMiddleware))]
    public async Task Invoke_ComCorrelationIdNoHeader_PreservaCorrelationIdExistente()
    {
        const string correlationExistente = "meu-correlation-id-existente";
        RequestDelegate next = ctx => Task.CompletedTask;
        var logger = NullLogger<HandlingHeadersMiddleware>.Instance;
        var middleware = new HandlingHeadersMiddleware(next, logger, BuildOptions());

        var context = new DefaultHttpContext();
        context.Request.Headers[Constants.CorrelationHeader] = correlationExistente;

        await middleware.Invoke(context);

        var headerRetornado = context.Response.Headers[Constants.CorrelationHeader].ToString();
        Assert.Equal(correlationExistente, headerRetornado);
    }

    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(HandlingHeadersMiddleware))]
    public async Task Invoke_AdicionaHeaderDeVersao()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var logger = NullLogger<HandlingHeadersMiddleware>.Instance;
        var middleware = new HandlingHeadersMiddleware(next, logger, BuildOptions());
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.True(context.Response.Headers.ContainsKey("X-AssemblyVersion"));
    }

    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(HandlingHeadersMiddleware))]
    public async Task Invoke_NextEhChamado()
    {
        var nextChamado = false;
        RequestDelegate next = ctx =>
        {
            nextChamado = true;
            return Task.CompletedTask;
        };

        var logger = NullLogger<HandlingHeadersMiddleware>.Instance;
        var middleware = new HandlingHeadersMiddleware(next, logger, BuildOptions());
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.True(nextChamado);
    }
}
