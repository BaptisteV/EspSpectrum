namespace EspSpectrum.Core.Recording.TimingMonitoring;

public interface ITickTimingMonitor
{
    Task LogSummaryLoop();
    void NotifyLoopDone(DateTimeOffset dt);
}
