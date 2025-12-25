using NAudio.Dsp;

namespace EspSpectrum.Core.Fft;

public class FftProcessor(int sampleRate)
{
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
            var startBin = FrequencyToBin(Bands.FrequencyBands.ElementAt(band), binFrequencyResolution);
            var endBin = FrequencyToBin(Bands.FrequencyBands.ElementAt(band + 1), binFrequencyResolution);

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

    private readonly int _sampleRate = sampleRate;

    private readonly Complex[] fftBuffer = new Complex[FftProps.FftLength];

    private double[] ProcessFFTBands(ReadOnlySpan<float> sample)
    {
        for (var i = 0; i < FftProps.FftLength; i++)
        {
            fftBuffer[i].X = sample[i] * Bands.HammingWindow[i];
            fftBuffer[i].Y = 0f;
        }

        FastFourierTransform.FFT(true, FftPow, fftBuffer);

        var bands = CalculateBands(fftBuffer, _sampleRate);
        return bands;
    }

    public Spectrum ToFft(ReadOnlySpan<float> sample)
    {
        var bands = ProcessFFTBands(sample);
        var volume = GetVolume(sample);
        return new Spectrum
        {
            Bands = bands,
            Volume = volume,
        };
    }

    private double CalculateAmplitude(ReadOnlySpan<float> buffer, int length)
    {
        var sum = 0.0;

        for (int i = 0; i < length; i++)
        {
            sum += MathF.Abs(buffer[i]);
        }

        return sum / length;
    }

    private double AmplitudeToDecibels(double amplitude)
    {
        if (amplitude <= 0)
            return -160; // Minimum practical dB value

        // Reference amplitude for 16-bit audio
        const double reference = 32768.0;
        return 20 * Math.Log10(amplitude / reference);
    }

    private double GetVolume(ReadOnlySpan<float> sample)
    {
        var amplitude = CalculateAmplitude(sample, sample.Length);
        var db = AmplitudeToDecibels(amplitude);

        return db;
    }
}
