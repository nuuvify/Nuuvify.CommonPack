using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public class InMemoryMftIdempotencyStoreTests
{
    [Fact]
    public async Task TryStart_DeveRetornarFalseQuandoChaveJaExiste()
    {
        var store = new InMemoryMftIdempotencyStore();
        var key = "integration|correlation|item|file";

        var first = await store.TryStartAsync(key);
        var second = await store.TryStartAsync(key);

        Assert.True(first);
        Assert.False(second);
    }
}
