using Nuuvify.CommonPack.MftMailbox.Configuration;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Configuration;

[Trait("Category", "Unit")]
public class MftMailboxOptionsTests
{
    [Fact]
    public void DefaultsDevemAtenderLimitesDoPlano()
    {
        var options = new MftMailboxOptions();

        Assert.Equal(500, options.MaxBatchSize);
        Assert.Equal(1_073_741_824, options.MaxFileSizeBytes);
        Assert.Equal(InboundFileOrdering.None, options.InboundFileOrdering);
    }
}
