using NAudio.Dsp;

namespace EspSpectrum.Core.Fft;

public static class Bands
{
    public static IReadOnlyCollection<float> FrequencyBands { get; private set; } = [];
    public static float[] HammingWindow { get; } = new float[FftProps.FftLength];

    static Bands()
    {
        InitializeBandBoundaries();
        InitializeHammingWindow();
    }

    private static void InitializeBandBoundaries()
    {
        var freq = new float[FftProps.NBands + 1];
        for (var i = 0; i < freq.Length; i++)
        {
            var t = i / (freq.Length - 1);
            freq[i] = FftProps.MinFreq * MathF.Pow(FftProps.MaxFreq / FftProps.MinFreq, t);
        }
        FrequencyBands = freq.AsReadOnly();
    }

    private static void InitializeHammingWindow()
    {
        for (var i = 0; i < FftProps.FftLength; i++)
        {
            HammingWindow[i] = (float)FastFourierTransform.HammingWindow(i, FftProps.FftLength);
        }
    }
}
