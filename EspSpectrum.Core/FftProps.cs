namespace EspSpectrum.Core;

public static class FftProps
{
    public const int ReadLength = 700;

    public const int FftLength = 4096;

    public const int BandHeigth = 8;
    public const int NBands = 32;
    public const float MinFreq = 60f;
    public const float MaxFreq = 12_000f;
    public const float ScaleFactor = 8.0f;

    public static TimeSpan WaitForAudioTightLoop { get; } = TimeSpan.FromMicroseconds(8);
}
