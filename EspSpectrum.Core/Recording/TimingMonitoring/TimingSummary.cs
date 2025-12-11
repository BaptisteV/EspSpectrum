namespace EspSpectrum.Core.Recording.TimingMonitoring;

public class TimingSummary
{
    public Timing OnTime { get; set; } = new();

    public Timing Late { get; set; } = new();
}
