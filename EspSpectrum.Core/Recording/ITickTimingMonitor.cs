using Microsoft.Extensions.Logging;

namespace EspSpectrum.Core.Recording;

public interface ITickTimingMonitor
{
    void StartInBg();
    void NotifyTickDiff(TimeSpan diff);
    TimingSummary Summary();
}

public class Timing
{
    public TimeSpan Average { get; set; }
    public TimeSpan Min { get; set; }
    public TimeSpan Max { get; set; }
    public int Count { get; set; }

    public override string ToString()
    {
        return $"Average: {Average.TotalMilliseconds:n2}\tMin: {Min.TotalMilliseconds:n2}\tMax: {Max.TotalMilliseconds}\tCount: {Count}";
    }
}
public class TimingSummary
{
    public Timing OnTime { get; set; } = new();

    public Timing Late { get; set; } = new();
}

public sealed class TimingMonitor : ITickTimingMonitor, IDisposable
{
    private readonly TimeSpan DeleteAfter = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(2);

    public class TimingMesurement
    {
        public TimeSpan TimeLeft { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

    private readonly List<TimingMesurement> mesurements = [];

    private readonly System.Timers.Timer LogTimer = new(LogInterval)
    {
        AutoReset = true
    };

    private readonly ILogger<TimingMonitor> _logger;

    public TimingMonitor(ILogger<TimingMonitor> logger)
    {
        _logger = logger;

        LogTimer.Elapsed += (sender, args) =>
        {
            var summary = Summary();
            _logger.LogInformation("Late   : {LateSummary}", summary.Late);
            _logger.LogInformation("On time: {OnTimeSummary}", summary.OnTime);
        };
    }

    private void CleanupOldMeasurements()
    {
        var now = DateTimeOffset.UtcNow;
        mesurements.RemoveAll(m => (now - m.TimeStamp) > DeleteAfter);
    }

    public void NotifyTickDiff(TimeSpan diff)
    {
        _logger.LogTrace("Tick was late by {TimeLeft:n2}ms", diff.TotalMilliseconds);
        mesurements.Add(new TimingMesurement
        {
            TimeLeft = diff,
            TimeStamp = DateTimeOffset.UtcNow,
        });

        CleanupOldMeasurements();
    }

    public TimingSummary Summary()
    {
        var tolerance = TimeSpan.FromMilliseconds(1);
        var ordered = mesurements
            .OrderBy(mesurements => mesurements.TimeLeft);

        var lates = ordered
            .TakeWhile(m => m.TimeLeft < -tolerance / 2.0)
            .ToList();

        var onTime = ordered
            .TakeLast(mesurements.Count - lates.Count)
            .ToList();

        var a = new TimingSummary()
        {
            OnTime = onTime.Count == 0 ? new Timing() :
            new Timing()
            {
                Average = TimeSpan.FromTicks((long)onTime.Average(m => m.TimeLeft.Ticks)),
                Min = onTime.Min(m => m.TimeLeft),
                Max = onTime.Max(m => m.TimeLeft),
                Count = onTime.Count,
            },

            Late = lates.Count == 0 ? new Timing() :
            new Timing()
            {
                Average = TimeSpan.FromTicks((long)lates.Average(m => m.TimeLeft.Ticks)),
                Min = lates.Min(m => m.TimeLeft),
                Max = lates.Max(m => m.TimeLeft),
                Count = lates.Count,
            }
        };
        return a;
    }

    public void StartInBg()
    {
        LogTimer.Start();

    }

    public void Dispose()
    {
        LogTimer.Dispose();
    }
}