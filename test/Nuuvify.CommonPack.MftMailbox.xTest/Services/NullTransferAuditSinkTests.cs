using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Services;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Services;

[Trait("Category", "Unit")]
public class NullTransferAuditSinkTests
{
    [Fact]
    public async Task WriteAsync_ComEntradaNula_DeveLancarArgumentNullException()
    {
        var sink = new NullTransferAuditSink();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sink.WriteAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task WriteAsync_ComEntradaValida_DeveCompletarSemErro()
    {
        var sink = new NullTransferAuditSink();

        var entry = new TransferAuditEntry
        {
            IntegrationKey = "integration",
            ItemId = "item"
        };

        await sink.WriteAsync(entry, CancellationToken.None);
    }
}
