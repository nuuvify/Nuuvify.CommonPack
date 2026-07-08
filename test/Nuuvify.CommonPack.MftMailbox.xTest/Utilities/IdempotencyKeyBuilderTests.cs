using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Utilities;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Utilities;

[Trait("Category", "Unit")]
public class IdempotencyKeyBuilderTests
{
    [Fact]
    public void Build_DeveGerarChaveDeterministica()
    {
        var envelope = new TransferEnvelope
        {
            IntegrationKey = "erp-a",
            CorrelationId = "corr-123"
        };

        var item = new TransferItem
        {
            ItemId = "item-001",
            FileName = "pedido.csv"
        };

        var key = IdempotencyKeyBuilder.Build(envelope, item);

        Assert.Equal("erp-a|corr-123|item-001|pedido.csv", key);
    }
}
