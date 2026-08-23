using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.Middleware.Setups;
using Nuuvify.CommonPack.Observability;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class OperationContextHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PopulatesContextAndRestoresPreviousValue()
    {
        var accessor = new OperationContextAccessor
        {
            Current = new Nuuvify.CommonPack.Observability.Abstraction.OperationContext("outer")
        };
        var contextDuringNext = string.Empty;
        RequestDelegate next = _ =>
        {
            contextDuringNext = accessor.Current.CorrelationId;
            return Task.CompletedTask;
        };
        var middleware = new OperationContextHeadersMiddleware(
            next,
            accessor,
            NullLogger<OperationContextHeadersMiddleware>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.CorrelationHeader] = "request-correlation";

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("request-correlation", contextDuringNext);
        Assert.Equal("request-correlation", httpContext.Response.Headers[Constants.CorrelationHeader].ToString());
        Assert.Equal("outer", accessor.Current.CorrelationId);
    }
}
