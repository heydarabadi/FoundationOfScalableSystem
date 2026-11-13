using System.Diagnostics;
using System.Globalization;

namespace Hpt.Api;

/// <summary>
/// Samples this API process' CPU use relative to its container CPU quota.
/// The latest value is used for the ORCA load report sent to Envoy.
/// </summary>
public sealed class CpuLoadSampler(ILogger<CpuLoadSampler> logger) : BackgroundService
{
    private double _utilization;

    public double Utilization => Volatile.Read(ref _utilization);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cpuCapacityInCores = ReadCpuQuotaInCores() ?? Math.Max(1, Environment.ProcessorCount);
        using var process = Process.GetCurrentProcess();
        var previousCpuTime = process.TotalProcessorTime;
        var previousTimestamp = Stopwatch.GetTimestamp();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                process.Refresh();
                var currentCpuTime = process.TotalProcessorTime;
                var currentTimestamp = Stopwatch.GetTimestamp();
                var elapsedSeconds = Stopwatch.GetElapsedTime(previousTimestamp, currentTimestamp).TotalSeconds;
                var cpuSeconds = (currentCpuTime - previousCpuTime).TotalSeconds;

                if (elapsedSeconds > 0 && cpuSeconds >= 0)
                {
                    var fraction = cpuSeconds / elapsedSeconds / cpuCapacityInCores;
                    Volatile.Write(ref _utilization, Math.Clamp(fraction, 0, 1));
                }

                previousCpuTime = currentCpuTime;
                previousTimestamp = currentTimestamp;
            }
            catch (Exception exception) when (exception is InvalidOperationException or
                                               System.ComponentModel.Win32Exception or
                                               IOException)
            {
                logger.LogWarning(exception, "Could not sample API process CPU utilization; keeping the last sample.");
            }
        }
    }

    private static double? ReadCpuQuotaInCores()
    {
        // cgroup v2: "max 100000" or "50000 100000" (0.5 CPU cores).
        const string cgroupV2CpuMax = "/sys/fs/cgroup/cpu.max";
        if (File.Exists(cgroupV2CpuMax))
        {
            var fields = File.ReadAllText(cgroupV2CpuMax).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 2 && fields[0] != "max" &&
                long.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quota) &&
                long.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var period) &&
                quota > 0 && period > 0)
            {
                return (double)quota / period;
            }
        }

        // cgroup v1 fallback.
        const string quotaPath = "/sys/fs/cgroup/cpu/cpu.cfs_quota_us";
        const string periodPath = "/sys/fs/cgroup/cpu/cpu.cfs_period_us";
        if (File.Exists(quotaPath) && File.Exists(periodPath) &&
            long.TryParse(File.ReadAllText(quotaPath).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v1Quota) &&
            long.TryParse(File.ReadAllText(periodPath).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v1Period) &&
            v1Quota > 0 && v1Period > 0)
        {
            return (double)v1Quota / v1Period;
        }

        return null;
    }
}
