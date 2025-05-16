namespace EspSpectrum.Core;

public static class FftProps
{
    public const int ReadLength = 4096 / 4;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const double MinFreq = 50.0;
    public const double MaxFreq = 10_000.0;
    public const double Amplification = 1.0;
    public const double ScaleFactor20 = 80.0;

    public static TimeSpan WaitForAudioTightLoop { get; } = TimeSpan.FromMicroseconds(1);
}
