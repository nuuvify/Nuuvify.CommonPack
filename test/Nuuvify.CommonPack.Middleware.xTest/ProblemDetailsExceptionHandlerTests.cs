using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Middleware.Setups;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class ProblemDetailsExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WritesGenericProblemDetailsWithoutExceptionMessage()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetailsExceptionHandler();
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IExceptionHandler>();
        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext
        {
            Response = { Body = responseBody }
        };

        var result = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("segredo interno"),
            default);

        var body = Encoding.UTF8.GetString(responseBody.ToArray());
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.DoesNotContain("segredo interno", body, StringComparison.Ordinal);
        Assert.Contains("An unexpected error occurred.", body, StringComparison.Ordinal);
    }
}
