namespace EspSpectrum.Core;

public static class FftProps
{
    public const int ReadLength = 512;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const float MinFreq = 60f;
    public const float MaxFreq = 12_000f;
    public const double ScaleFactor = 8d;

    public static TimeSpan WaitForAudioTightLoop { get; } = TimeSpan.FromMicroseconds(100);
}
