using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nuuvify.CommonPack.HealthCheck;

public class StorageMetrics
{
    public double Total { get; set; }
    public double Used { get; set; }
    public double Free { get; set; }
}

public class MemoryMetricsClient
{

    public StorageMetrics GetMetrics()
    {
        StorageMetrics metrics;

        try
        {

            if (IsUnix())
            {
                metrics = GetUnixMetrics();
            }
            else
            {
                metrics = GetWindowsMetrics();
            }

            return metrics;
        }
        catch (Exception)
        {
            metrics = new StorageMetrics()
            {
                Free = 2,
                Total = 3,
                Used = 1,
            };

            return metrics;
        }
    }

    private bool IsUnix()
    {
        var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        return isUnix;
    }

    private StorageMetrics GetWindowsMetrics()
    {
        var output = string.Empty;

        var info = new ProcessStartInfo
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
        }

        var lines = output.Trim().Split('\n');
        var freeMemoryParts = lines[0].Split('=', StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split('=', StringSplitOptions.RemoveEmptyEntries);

        var metrics = new StorageMetrics
        {
            Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
            Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
        };
        metrics.Used = metrics.Total - metrics.Free;

        return metrics;
    }

    private StorageMetrics GetUnixMetrics()
    {
        var output = "";

        var info = new ProcessStartInfo("free -m")
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(info))
        {
            output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }

        var lines = output.Split('\n');
        var memory = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var metrics = new StorageMetrics
        {
            Total = double.Parse(memory[1]),
            Used = double.Parse(memory[2]),
            Free = double.Parse(memory[3])
        };

        return metrics;
    }
}
