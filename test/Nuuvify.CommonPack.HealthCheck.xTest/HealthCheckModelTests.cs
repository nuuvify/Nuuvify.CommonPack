using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

[Trait("Category", "Unit")]
public sealed class HealthCheckModelTests
{
    [Fact]
    public void DataEntries_WhenEntriesContainData_ShouldFlattenData()
    {
        var report = new HealthReportCustom
        {
            Entries = new List<HealthReportEntryCustom>
            {
                new()
                {
                    Name = "api",
                    Data = new Dictionary<string, object> { ["latency"] = 12 }
                }
            }
        };

        var result = report.DataEntries();

        result["latency"].ShouldBe(12);
    }

    [Fact]
    public void DataEntries_WhenEntryHasNoData_ShouldCreateSummaryValue()
    {
        var report = new HealthReportCustom
        {
            Entries = new List<HealthReportEntryCustom>
            {
                new()
                {
                    Name = "api",
                    Status = "Unhealthy",
                    Description = "offline",
                    ExceptionMessage = "timeout",
                    Data = new Dictionary<string, object>()
                }
            }
        };

        var result = report.DataEntries();

        result["api"].ShouldBe("Unhealthy offline timeout");
    }

    [Fact]
    public void DataEntries_WhenKeysRepeat_ShouldKeepFirstValue()
    {
        var report = new HealthReportCustom
        {
            Entries = new List<HealthReportEntryCustom>
            {
                new() { Data = new Dictionary<string, object> { ["key"] = "first" } },
                new() { Data = new Dictionary<string, object> { ["key"] = "second" } }
            }
        };

        report.DataEntries()["key"].ShouldBe("first");
    }

    [Fact]
    public void Exception_WhenAssigned_ShouldExposeExceptionMessage()
    {
        var entry = new HealthReportEntryCustom
        {
            Exception = new InvalidOperationException("failure")
        };

        entry.ExceptionMessage.ShouldBe("failure");
        entry.Exception.ShouldBeNull();
    }

    [Fact]
    public async Task MemoryHealthCheck_ShouldReturnMetricsData()
    {
        var result = await new MemoryHealthCheck().CheckHealthAsync(new HealthCheckContext());

        result.Data.ContainsKey("Total").ShouldBeTrue();
        result.Data.ContainsKey("Used").ShouldBeTrue();
        result.Data.ContainsKey("Free").ShouldBeTrue();
        result.Status.ShouldBeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task LocalStorageHealthCheck_ShouldReturnMetricsData()
    {
        var result = await new LocalStorageHealthCheck().CheckHealthAsync(new HealthCheckContext());

        result.Data.ContainsKey("Total GB").ShouldBeTrue();
        result.Data.ContainsKey("Used GB").ShouldBeTrue();
        result.Data.ContainsKey("Free GB").ShouldBeTrue();
        result.Status.ShouldBeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
    }
}
