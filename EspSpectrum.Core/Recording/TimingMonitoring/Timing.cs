namespace EspSpectrum.Core.Recording.TimingMonitoring;

public class Timing
{
    public TimeSpan Average { get; set; }
    public TimeSpan Min { get; set; }
    public TimeSpan Max { get; set; }
    public int Count { get; set; }
    public double StandardDeviation { get; set; }

    public override string ToString()
    {
        return $"Count={Count}\tAverage={Average.TotalMilliseconds:n2}\tMin={Min.TotalMilliseconds:n2}\tMax={Max.TotalMilliseconds}\tStandard deviation={StandardDeviation:n2}";
    }
}
