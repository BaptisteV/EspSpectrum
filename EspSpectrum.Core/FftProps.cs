namespace EspSpectrum.Core;

public static class FftProps
{
    public const int ReadLength = 512;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const double MinFreq = 60.0;
    public const double MaxFreq = 12_000.0;
    public const double ScaleFactor = 4.0;

    public static TimeSpan WaitForAudioTightLoop { get; } = TimeSpan.FromMicroseconds(100);
}
