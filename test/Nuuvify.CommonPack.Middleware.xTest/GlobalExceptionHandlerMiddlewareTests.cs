using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nuuvify.CommonPack.Middleware.Handle;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(GlobalExceptionHandlerMiddleware))]
    public async Task Invoke_SemExcecao_NextEhChamado()
    {
        var nextChamado = false;
        RequestDelegate next = ctx =>
        {
            nextChamado = true;
            return Task.CompletedTask;
        };

        var mockHandler = new Mock<IGlobalHandleException>();
        var middleware = new GlobalExceptionHandlerMiddleware(next, mockHandler.Object);
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.True(nextChamado);
        mockHandler.Verify(h => h.HandleException(It.IsAny<Exception>(), It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    [Trait("CommonApi.Middleware-Setups", nameof(GlobalExceptionHandlerMiddleware))]
    public async Task Invoke_ComExcecao_HandleExceptionEhChamado()
    {
        var excecao = new InvalidOperationException("erro de teste");
        RequestDelegate next = _ => throw excecao;

        var mockHandler = new Mock<IGlobalHandleException>();
        mockHandler.Setup(h => h.HandleException(It.IsAny<Exception>(), It.IsAny<HttpContext>()))
                   .Returns(Task.CompletedTask);

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockHandler.Object);
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        mockHandler.Verify(h => h.HandleException(excecao, context), Times.Once);
    }
}
