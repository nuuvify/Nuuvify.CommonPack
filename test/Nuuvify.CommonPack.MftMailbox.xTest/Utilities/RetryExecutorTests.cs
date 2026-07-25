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

    [Fact]
    public async Task ExecuteAsync_ComMaxRetriesNegativo_DeveFalharComArgumentOutOfRangeException()
    {
        var options = new RetryOptions
        {
            MaxRetries = -1,
            BaseDelay = TimeSpan.FromMilliseconds(1),
            MaxDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            RetryExecutor.ExecuteAsync(
                () => Task.FromResult(1),
                _ => true,
                options,
                CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_ComBaseDelayMaiorQueMaxDelay_DeveFalharComArgumentOutOfRangeException()
    {
        var options = new RetryOptions
        {
            MaxRetries = 1,
            BaseDelay = TimeSpan.FromSeconds(5),
            MaxDelay = TimeSpan.FromSeconds(1),
            UseJitter = true
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            RetryExecutor.ExecuteAsync(
                () => Task.FromResult(1),
                _ => true,
                options,
                CancellationToken.None));
    }
}
