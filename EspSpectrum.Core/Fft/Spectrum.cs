namespace EspSpectrum.Core.Fft;

public class Spectrum
{
    public required double[] Bands { get; set; } = new double[FftProps.NBands];
    public required double Volume { get; set; } = 0.0;
}
