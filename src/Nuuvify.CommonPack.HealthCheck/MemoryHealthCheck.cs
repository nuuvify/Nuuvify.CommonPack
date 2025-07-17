using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nuuvify.CommonPack.HealthCheck;

public class MemoryHealthCheck : IHealthCheck
{

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        var client = new MemoryMetricsClient();
        var metrics = client.GetMetrics();
        var percentUsed = 100 * metrics.Used / metrics.Total;
        percentUsed = Math.Round(percentUsed, 2);


        var status = HealthStatus.Healthy;
        var description = $"Memory used is normal: {percentUsed}";

        if (percentUsed > 80)
        {
            status = HealthStatus.Degraded;
            description = "More than 80% of memory is being used";
        }
        if (percentUsed > 90)
        {
            status = HealthStatus.Unhealthy;
            description = "More than 90% of memory is being used";

        }

        var data = new Dictionary<string, object>
            {
                { "Total", metrics.Total },
                { "Used", metrics.Used },
                { "Free", metrics.Free }
            };

        var result = new HealthCheckResult(status: status, description: description, data: data);

        return await Task.FromResult(result);
    }
}
