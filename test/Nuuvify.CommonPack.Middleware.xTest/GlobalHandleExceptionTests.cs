using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Nuuvify.CommonPack.Middleware.Handle;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

public class GlobalHandleExceptionTests
{

    [Fact]
    public async Task ExceptionGlobalDeveLogarException()
    {

        var exceptionFake = new Exception("Isso é um teste");

        var testeException = new GlobalHandleException(new NullLogger<GlobalHandleException>());
        var taskException = testeException.HandleException(exceptionFake, new DefaultHttpContext());

        await Task.CompletedTask;

        Assert.True(taskException.IsCompleted);
    }
}
