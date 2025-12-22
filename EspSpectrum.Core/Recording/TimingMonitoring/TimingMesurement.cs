namespace EspSpectrum.Core.Recording.TimingMonitoring;

public sealed partial class TimingMonitor
{
    public class TimingMesurement
    {
        public TimeSpan TimeLeft { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}