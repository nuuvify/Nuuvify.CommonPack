using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nuuvify.CommonPack.HealthCheck
{
    public class LocalStorageHealthCheck : IHealthCheck
    {

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var metrics = new StorageMetrics();


            var allDrives = DriveInfo.GetDrives()
                                     .Where(x => x.DriveType == DriveType.Fixed);


            foreach (var d in allDrives)
            {
                if (d.IsReady)
                {
                    metrics.Free += d.TotalFreeSpace;
                    metrics.Total += d.TotalSize;
                }
            }

            metrics.Free = Math.Round(metrics.Free / 1024 / 1024 / 1024, 0);
            metrics.Total = Math.Round(metrics.Total / 1024 / 1024 / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;
            var percentUsed = 100 * metrics.Used / metrics.Total;


            var status = HealthStatus.Healthy;
            var description = $"Storage used is normal: {metrics.Used} GB";

            if (percentUsed > 80)
            {
                status = HealthStatus.Degraded;
                description = $"More than 80% of Storage is being used: {metrics.Used} GB";
            }
            if (percentUsed > 90)
            {
                status = HealthStatus.Unhealthy;
                description = $"More than 90% of Storage is being used: {metrics.Used} GB";

            }

            var data = new Dictionary<string, object>
            {
                { "Total GB", metrics.Total },
                { "Used GB", metrics.Used },
                { "Free GB", metrics.Free }
            };

            var result = new HealthCheckResult(status: status, description: description, data: data);

            return await Task.FromResult(result);
        }
    }
}