using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using static EspSpectrum.Core.Recording.TimingMonitoring.TimingMonitor;

namespace EspSpectrum.Core.Recording.TimingMonitoring;

public class AsyncTimingMonitor : ITickTimingMonitor, IDisposable
{
    private static readonly int HISTO_SIZE = 500;
    private readonly ConcurrentQueue<TimingMesurement> _mesurements = new();
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(2);
    private readonly Stopwatch _sw = new();

    private readonly CancellationTokenSource _cts;
    private readonly ILogger<AsyncTimingMonitor> _logger;

    public AsyncTimingMonitor(ILogger<AsyncTimingMonitor> logger)
    {
        _cts = new CancellationTokenSource();
        _logger = logger;
    }

    private TimingSummary ComputeSummary()
    {
        var l = _mesurements.ToList();
        var diffs = new List<TimeSpan>();
        for (var i = 0; i < l.Count - 1; i++)
        {
            diffs.Add(l[i + 1].TimeStamp - l[i].TimeStamp);
        }
        var summary = new TimingSummary()
        {
            Summary = l.Count == 0 ? new Timing() :
                new Timing()
                {
                    // Remove outliers > 1sec
                    Average = TimeSpan.FromTicks((long)diffs.Where(t => t.TotalSeconds < 1).Select(t => t.Ticks).Average()),
                    Min = diffs.Min(),
                    Max = diffs.Max(),
                    Count = diffs.Count,
                },
        };
        return summary;
    }

    private void LogSummary()
    {
        var summary = ComputeSummary();
        _logger.LogInformation("Timing Summary - OnTime: Count={OnTimeCount}, Avg={OnTimeAvg:n2}ms, Min={OnTimeMin:n2}ms, Max={OnTimeMax:n2}ms",
            summary.Summary.Count,
            summary.Summary.Average.TotalMilliseconds,
            summary.Summary.Min.TotalMilliseconds,
            summary.Summary.Max.TotalMilliseconds);
    }

    private void CleanupOldMeasurements()
    {
        while (_mesurements.Count > HISTO_SIZE)
        {
            _mesurements.TryDequeue(out var _);
        }
    }

    private DateTimeOffset lastDt = DateTimeOffset.MinValue;
    public void NotifyFFTSent(DateTimeOffset dt)
    {
        var sentAfter = dt - lastDt;
        _logger.LogTrace("FFT Sent after {FFTTime:n2}ms", sentAfter.TotalMilliseconds);
        lastDt = dt;

        _mesurements.Enqueue(new TimingMesurement
        {
            TimeLeft = sentAfter,
            TimeStamp = dt,
        });

        CleanupOldMeasurements();
    }

    public Task LogSummaryLoop()
    {
        _ = Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(LogInterval);
            while (await periodicTimer.WaitForNextTickAsync())
            {
                LogSummary();
            }
        });
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
