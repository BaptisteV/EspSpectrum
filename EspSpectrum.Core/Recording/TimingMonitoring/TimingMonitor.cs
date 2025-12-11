using Microsoft.Extensions.Logging;

namespace EspSpectrum.Core.Recording.TimingMonitoring;

public sealed class TimingMonitor(ILogger<TimingMonitor> logger) : ITickTimingMonitor, IDisposable
{
    private readonly ILogger<TimingMonitor> _logger = logger;
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