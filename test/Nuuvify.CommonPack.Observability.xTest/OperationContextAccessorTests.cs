using Nuuvify.CommonPack.Observability;
using Nuuvify.CommonPack.Observability.Abstraction;
using Xunit;

namespace Nuuvify.CommonPack.Observability.xTest;

[Trait("Category", "Unit")]
public class OperationContextAccessorTests
{
    [Fact]
    public void Current_DefaultsToEmptyContext()
    {
        var accessor = new OperationContextAccessor();

        Assert.NotNull(accessor.Current);
        Assert.Equal(string.Empty, accessor.Current.CorrelationId);
        Assert.Equal(string.Empty, accessor.Current.TraceId);
        Assert.Equal(string.Empty, accessor.Current.OperationId);
    }

    [Fact]
    public async Task ConcurrentScopes_KeepTheirOwnOperationContext()
    {
        var accessor = new OperationContextAccessor();
        var first = new OperationContext("corr-1", "trace-1", "op-1");
        var second = new OperationContext("corr-2", "trace-2", "op-2");

        var task1 = Task.Run(() =>
        {
            using var scope = new OperationContextScope(accessor, first);
            Assert.Equal("corr-1", accessor.Current.CorrelationId);
            await Task.Delay(50);
            Assert.Equal("trace-1", accessor.Current.TraceId);
        });

        var task2 = Task.Run(() =>
        {
            using var scope = new OperationContextScope(accessor, second);
            Assert.Equal("corr-2", accessor.Current.CorrelationId);
            await Task.Delay(50);
            Assert.Equal("trace-2", accessor.Current.TraceId);
        });

        await Task.WhenAll(task1, task2);
    }
}
