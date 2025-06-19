using NAudio.Dsp;

namespace EspSpectrum.Core.Fft;

public class FftProcessor
{
    public static IReadOnlyCollection<double> FrequencyBands { get; private set; } = [];

    public FftProcessor(int sampleRate)
    {
        InitializeBandBoundaries();
        // Pre-calculate Hamming window
        HammingWindow = new float[FftProps.FftLength];
        for (var i = 0; i < FftProps.FftLength; i++)
        {
            HammingWindow[i] = (float)FastFourierTransform.HammingWindow(i, FftProps.FftLength);
        }

        _sampleRate = sampleRate;
    }

    private static void InitializeBandBoundaries()
    {
        var freq = new double[FftProps.NBands + 1];
        for (var i = 0; i < freq.Length; i++)
        {
            var t = (double)i / (freq.Length - 1);
            freq[i] = FftProps.MinFreq * Math.Pow(FftProps.MaxFreq / FftProps.MinFreq, t);
        }
        FrequencyBands = freq.AsReadOnly();
    }

    private static int FrequencyToBin(double frequency, double binResolution)
    {
        return (int)Math.Round(Math.Clamp(frequency / binResolution, 0.0, FftProps.FftLength / 2.0 - 1.0));
    }

    private static double[] CalculateBands(Complex[] fftResult, int sampleRate)
    {
        var bandLevels = new double[FftProps.NBands];
        var binFrequencyResolution = (double)sampleRate / FftProps.FftLength;

        for (var band = 0; band < FftProps.NBands; band++)
        {
            // Find the FFT bins corresponding to this band's frequency range
            var startBin = FrequencyToBin(FrequencyBands.ElementAt(band), binFrequencyResolution);
            var endBin = FrequencyToBin(FrequencyBands.ElementAt(band + 1), binFrequencyResolution);

            // Calculate band energy
            var bandEnergy = 0d;
            for (var bin = startBin; bin < endBin; bin++)
            {
                // Calculate magnitude (energy) of the complex FFT result
                bandEnergy += Math.Sqrt(
                    fftResult[bin].X * fftResult[bin].X +
                    fftResult[bin].Y * fftResult[bin].Y
                );
            }

            // Apply logarithmic scaling
            bandLevels[band] = Math.Log10(bandEnergy + 1) * FftProps.ScaleFactor20;
        }

        return bandLevels;
    }

    private static readonly int FftPow = (int)Math.Log(FftProps.FftLength, 2.0);

    private readonly float[] HammingWindow;
    private readonly int _sampleRate;

    private readonly Complex[] fftBuffer = new Complex[FftProps.FftLength];

    public Spectrum ToFft(ReadOnlySpan<float> sample)
    {
        for (var i = 0; i < FftProps.FftLength; i++)
        {
            fftBuffer[i].X = sample[i] * HammingWindow[i];
            fftBuffer[i].Y = 0f;
        }
        FastFourierTransform.FFT(true, FftPow, fftBuffer);
        var bands = CalculateBands(fftBuffer, _sampleRate);

        return new Spectrum() { Bands = bands };
    }

}
