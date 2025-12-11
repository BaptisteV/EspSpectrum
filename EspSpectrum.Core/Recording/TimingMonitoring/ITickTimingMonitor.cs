namespace EspSpectrum.Core.Recording.TimingMonitoring;

public interface ITickTimingMonitor
{
    void StartInBg();
    void NotifyTickDiff(TimeSpan diff);
    TimingSummary Summary();
}
