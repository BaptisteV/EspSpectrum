namespace EspSpectrum.Core.Recording.TimingMonitoring;

public interface ITickTimingMonitor
{
    Task StartInBg();
    void NotifyFFTSent(DateTimeOffset dt);
}
