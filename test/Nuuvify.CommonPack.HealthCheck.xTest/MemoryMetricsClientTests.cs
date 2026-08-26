using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

/// <summary>
/// Tests for memory metrics collection.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MemoryMetricsClientTests
{
    [Fact]
    public void GetMetrics_ShouldReturnNonNegativeMemoryValues()
    {
        var metrics = new MemoryMetricsClient().GetMetrics();

        metrics.ShouldNotBeNull();
        metrics.Total.ShouldBeGreaterThanOrEqualTo(0);
        metrics.Used.ShouldBeGreaterThanOrEqualTo(0);
        metrics.Free.ShouldBeGreaterThanOrEqualTo(0);
        (metrics.Used + metrics.Free).ShouldBeLessThanOrEqualTo(metrics.Total + 1);
    }

    [Fact]
    public void StorageMetrics_ShouldExposeAssignedValues()
    {
        var metrics = new StorageMetrics
        {
            Total = 10,
            Used = 6,
            Free = 4
        };

        metrics.Total.ShouldBe(10);
        metrics.Used.ShouldBe(6);
        metrics.Free.ShouldBe(4);
    }
}
