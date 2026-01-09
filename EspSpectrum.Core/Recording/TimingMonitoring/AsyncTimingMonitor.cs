using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EspSpectrum.Core.Recording.TimingMonitoring;

public sealed class AsyncTimingMonitor : ITickTimingMonitor, IDisposable
{
    private static readonly int HISTO_SIZE = 200;
    private readonly ConcurrentQueue<TimingMesurement> _mesurements = new();
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(2);

    private readonly CancellationTokenSource _cts;
    private readonly ILogger<AsyncTimingMonitor> _logger;

    public AsyncTimingMonitor(ILogger<AsyncTimingMonitor> logger)
    {
        _cts = new CancellationTokenSource();
        _logger = logger;
    }

    private Timing ComputeSummary()
    {
        var l = _mesurements.ToList();
        var diffs = new List<TimeSpan>();
        for (var i = 0; i < l.Count - 1; i++)
        {
            diffs.Add(l[i + 1].TimeStamp - l[i].TimeStamp);
        }

        // Remove outliers > 1sec
        var sampleForAverage = diffs.Where(t => t.TotalSeconds < 1).Select(t => t.Ticks);
        var average = sampleForAverage.Any() ? TimeSpan.FromTicks((long)sampleForAverage.Average()) : TimeSpan.Zero;

        return new Timing()
        {
            Average = average,
            Min = diffs.Min(),
            Max = diffs.Max(),
            Count = diffs.Count,
            StandardDeviation = diffs.Select(d => d.Ticks).StandardDeviation(),
        };
    }

    private void LogSummary()
    {
        // Need at least 2 measurements to compute diffs
        if (_mesurements.Count <= 1)
            return;

        var summary = ComputeSummary();
        _logger.LogInformation("Timing Summary: {TimingSummary}", summary);
    }

    private void CleanupOldMeasurements()
    {
        var dequeueCount = 0;
        var nDequeue = _mesurements.Count - HISTO_SIZE - 1;
        while (dequeueCount < nDequeue)
        {
            if (_mesurements.TryDequeue(out var _))
                dequeueCount++;
        }
    }

    private DateTimeOffset lastDt = DateTimeOffset.MinValue;

    public void NotifyFFTSent(DateTimeOffset dt)
    {
        var sentAfter = TimeSpan.FromTicks(Math.Abs(dt.Ticks - lastDt.Ticks));
        _logger.LogTrace("FFT Sent after {FFTTime:n2}ms", sentAfter.TotalMilliseconds);
        lastDt = dt;

        _mesurements.Enqueue(new TimingMesurement
        {
            TimeLeft = sentAfter,
            TimeStamp = dt,
        });

        CleanupOldMeasurements();
    }

    private readonly PeriodicTimer _logTimer = new(LogInterval);

    public Task LogSummaryLoop()
    {
        _ = Task.Run(async () =>
        {
            while (await _logTimer.WaitForNextTickAsync())
            {
                LogSummary();
            }
        }).ConfigureAwait(false);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _logTimer.Dispose();
        _cts.Cancel();
        _cts.Dispose();
    }
}

public static class Extend
{
    public static double StandardDeviation(this IEnumerable<long> values)
    {
        var avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
}