namespace EspSpectrum.Core;

public static class FftProps
{
    public const int ReadLength = 4096 / 8;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const double MinFreq = 50.0;
    public const double MaxFreq = 10_000.0;
    public const double ScaleFactor = 16.0;
    public const double ScaleFactor20 = 10.0;

    public static TimeSpan WaitForAudioTightLoop { get; } = TimeSpan.FromMicroseconds(100);
}
