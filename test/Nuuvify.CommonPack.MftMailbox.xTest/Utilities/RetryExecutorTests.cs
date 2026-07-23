using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Utilities;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Utilities;

[Trait("Category", "Unit")]
public class RetryExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetentarFalhasTransientesEAtingirSucesso()
    {
        var options = new RetryOptions
        {
            MaxRetries = 3,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(5),
            UseJitter = false
        };

        var count = 0;

        var result = await RetryExecutor.ExecuteAsync(
            () =>
            {
                count++;
                if (count < 3)
                {
                    throw new TimeoutException("transient");
                }

                return Task.FromResult(42);
            },
            ex => ex is TimeoutException,
            options,
            CancellationToken.None);

        Assert.Equal(42, result);
        Assert.Equal(3, count);
    }
}
