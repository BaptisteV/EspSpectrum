namespace EspSpectrum.Core.Recording.TimingMonitoring;
/*
public sealed partial class TimingMonitor(ILogger<TimingMonitor> logger) : ITickTimingMonitor, IDisposable
{
    private readonly ILogger<TimingMonitor> _logger = logger;
    private readonly TimeSpan DeleteAfter = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(2);

    private readonly ConcurrentQueue<TimingMesurement> _mesurements = [];

    private readonly System.Timers.Timer LogTimer = new(LogInterval)
    {
        AutoReset = true,
    };

    public TimingMonitor(ILogger<TimingMonitor> logger, TimeSpan deleteAfter, ConcurrentQueue<TimingMesurement> mesurements, System.Timers.Timer logTimer) : this(logger)
    {
        DeleteAfter = deleteAfter;
        this._mesurements = mesurements;
        LogTimer = logTimer;
        LogTimer.Elapsed += DoLog;
    }

    private void DoLog(object? sender, ElapsedEventArgs e)
    {
        LogSummary();
    }


    private void CleanupOldMeasurements()
    {
        var lelapsed = DateTimeOffset.UtcNow - _mesurements.LastOrDefault()?.TimeStamp;
        if (lelapsed < DeleteAfter)
        {
            return;
        }

        var recentEnough = false;
        // removed old measurement
        while (_mesurements.TryDequeue(out var m) && !recentEnough)
        {
            var elapsed = DateTimeOffset.UtcNow - m.TimeStamp;
            recentEnough = elapsed < DeleteAfter;
        }
    }

    public void NotifyTimeToNextTick(TimeSpan timeToNextTick)
    {
        _logger.LogDebug("Time to next tick {TimeLeft:n2}ms", timeToNextTick.TotalMilliseconds);
        if (timeToNextTick >= TimeSpan.Zero)
        {
            _mesurements.Enqueue(new TimingMesurement
            {
                TimeLeft = timeToNextTick,
                TimeStamp = DateTimeOffset.UtcNow,
            });
        }

        CleanupOldMeasurements();
    }

    private TimingSummary ComputeSummary()
    {
        var a = new TimingSummary()
        {
            Summary = _mesurements.Count == 0 ? new Timing() :
            new Timing()
            {
                Average = TimeSpan.FromTicks((long)_mesurements.Average(m => m.TimeLeft.Ticks)),
                Min = _mesurements.Min(m => m.TimeLeft),
                Max = _mesurements.Max(m => m.TimeLeft),
                Count = _mesurements.Count,
            },
        };
        return a;
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

    public Task StartInBg()
    {
        LogTimer.Start();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        LogTimer.Dispose();
    }
}*/