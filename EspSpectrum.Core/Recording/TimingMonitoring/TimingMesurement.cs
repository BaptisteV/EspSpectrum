namespace EspSpectrum.Core.Recording.TimingMonitoring;

public struct TimingMesurement
{
    public required TimeSpan TimeLeft { get; set; }
    public required DateTimeOffset TimeStamp { get; set; }
}