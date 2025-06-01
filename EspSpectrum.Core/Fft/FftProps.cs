namespace EspSpectrum.Core.Fft;

public static class FftProps
{
    public const int ReadLength = 4096 / 7;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const double MinFreq = 50.0;
    public const double MaxFreq = 15_000.0;
    public const double ScaleFactor20 = 80.0;
}
