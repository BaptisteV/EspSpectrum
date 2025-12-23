namespace EspSpectrum.Core.Recording.TimingMonitoring;

public interface ITickTimingMonitor
{
    Task LogSummaryLoop();
    void NotifyFFTSent(DateTimeOffset dt);
}
