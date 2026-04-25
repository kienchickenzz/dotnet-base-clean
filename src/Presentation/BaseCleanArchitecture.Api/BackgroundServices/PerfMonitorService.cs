/**
 * PerfMonitorService emits server performance metrics at regular intervals.
 *
 * <p>Runs as a background service, logging CPU %, RAM MB, and active request counts.
 * CPU is read from /proc/stat (Linux) or via Process for non-Linux fallback.</p>
 */
namespace BaseCleanArchitecture.Api.BackgroundServices;

using System.Diagnostics;

using BaseCleanArchitecture.Api.Monitoring;

/// <summary>
/// Background service that emits CPU %, RAM MB, and active request counts every 60 seconds.
/// </summary>
public sealed class PerfMonitorService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    private readonly ILogger<PerfMonitorService> _logger;

    private long _prevIdleTime;
    private long _prevTotalTime;
    private bool _isLinux;

    public PerfMonitorService(ILogger<PerfMonitorService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _isLinux = OperatingSystem.IsLinux();

        if (_isLinux)
        {
            _ReadLinuxCpuTimes(out _prevIdleTime, out _prevTotalTime);
        }

        // Let the host finish starting before the first snapshot
        await Task.Delay(Interval, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _EmitSnapshot();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("PerfMonitor snapshot failed: {Message}", ex.Message);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    #region Snapshot

    /// <summary>
    /// Emits a performance snapshot to the logger.
    /// </summary>
    private void _EmitSnapshot()
    {
        double cpuPercent = _MeasureCpu();

        using var process = Process.GetCurrentProcess();
        double totalMB = process.WorkingSet64 / (1024.0 * 1024.0);

        var gcInfo = GC.GetGCMemoryInfo();
        double gen0 = gcInfo.GenerationInfo.Length > 0 ? gcInfo.GenerationInfo[0].SizeAfterBytes / (1024.0 * 1024.0) : 0;
        double gen1 = gcInfo.GenerationInfo.Length > 1 ? gcInfo.GenerationInfo[1].SizeAfterBytes / (1024.0 * 1024.0) : 0;
        double gen2 = gcInfo.GenerationInfo.Length > 2 ? gcInfo.GenerationInfo[2].SizeAfterBytes / (1024.0 * 1024.0) : 0;
        double loh = gcInfo.GenerationInfo.Length > 3 ? gcInfo.GenerationInfo[3].SizeAfterBytes / (1024.0 * 1024.0) : 0;

        long activeQuery = RequestActivityCounter.ActiveQueryRequests;
        long activeIngestion = RequestActivityCounter.ActiveIngestionRequests;

        _logger.LogInformation(
            "[PerfMon] CPU: {Cpu:F1}% | Memory: {Total:F1} MB total, {Gen0:F1}, {Gen1:F1}, {Gen2:F1}, {Loh:F1} (Gen0/1/2/LOH MB) | Requests — query: {Query}, ingestion: {Ingestion}",
            cpuPercent,
            totalMB,
            gen0,
            gen1,
            gen2,
            loh,
            activeQuery,
            activeIngestion);
    }

    #endregion

    #region CPU Measurement

    /// <summary>
    /// Measures CPU usage using platform-specific method.
    /// </summary>
    private double _MeasureCpu()
    {
        if (_isLinux)
        {
            return _MeasureLinuxCpu();
        }
        return _MeasureProcessCpu();
    }

    /// <summary>
    /// Reads /proc/stat for system-wide CPU usage (the only reliable source in containers).
    /// Returns delta-based percentage since the previous call.
    /// </summary>
    private double _MeasureLinuxCpu()
    {
        _ReadLinuxCpuTimes(out long idle, out long total);

        long idleDelta = idle - _prevIdleTime;
        long totalDelta = total - _prevTotalTime;

        _prevIdleTime = idle;
        _prevTotalTime = total;

        if (totalDelta <= 0)
        {
            return 0;
        }

        return (1.0 - (double)idleDelta / totalDelta) * 100.0;
    }

    /// <summary>
    /// Parses the first "cpu" line from /proc/stat.
    /// Format: cpu  user nice system idle iowait irq softirq steal ...
    /// </summary>
    private static void _ReadLinuxCpuTimes(out long idle, out long total)
    {
        idle = 0;
        total = 0;

        try
        {
            string firstLine = File.ReadLines("/proc/stat").First();
            string[] parts = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // parts[0] == "cpu", parts[1..] are user, nice, system, idle, iowait, irq, softirq, steal, ...
            for (int i = 1; i < parts.Length; i++)
            {
                if (long.TryParse(parts[i], out long val))
                {
                    total += val;
                    if (i == 4) // idle column
                    {
                        idle = val;
                    }
                }
            }
        }
        catch
        {
            // Container might restrict /proc access; fall through with zeros
        }
    }

    /// <summary>
    /// Fallback for non-Linux (dev machines): rough per-process CPU estimate.
    /// </summary>
    private static double _MeasureProcessCpu()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            return process.TotalProcessorTime.TotalMilliseconds /
                   (Environment.ProcessorCount * process.UserProcessorTime.TotalMilliseconds + 1) * 100.0;
        }
        catch
        {
            return -1;
        }
    }

    #endregion
}
