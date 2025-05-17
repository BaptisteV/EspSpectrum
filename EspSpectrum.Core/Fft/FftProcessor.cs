using NAudio.Dsp;

namespace EspSpectrum.Core.Fft;

public class FftProcessor
{
    private static double[] _frequencyBands = [];

    public FftProcessor()
    {
        InitializeBandBoundaries();
    }

    private static void InitializeBandBoundaries()
    {
        _frequencyBands = new double[FftProps.NBands + 1];

        for (var i = 0; i < _frequencyBands.Length; i++)
        {
            var t = (double)i / (_frequencyBands.Length - 1);
            _frequencyBands[i] = FftProps.MinFreq * Math.Pow(FftProps.MaxFreq / FftProps.MinFreq, t);
        }
    }

    private static int FrequencyToBin(double frequency, double binResolution)
    {
        return (int)Math.Round(Math.Clamp(frequency / binResolution, 0.0, FftProps.FftLength / 2.0 - 1.0));
    }

    private int[] CalculateBands(Complex[] fftResult, int sampleRate)
    {
        var bandLevels = new int[FftProps.NBands];
        var binFrequencyResolution = (double)sampleRate / FftProps.FftLength;

        for (var band = 0; band < FftProps.NBands; band++)
        {
            // Find the FFT bins corresponding to this band's frequency range
            var startBin = FrequencyToBin(_frequencyBands[band], binFrequencyResolution);
            var endBin = FrequencyToBin(_frequencyBands[band + 1], binFrequencyResolution);

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
            bandLevels[band] = (int)Math.Round(Math.Log10(bandEnergy + 1) * FftProps.ScaleFactor20);
        }

        return bandLevels;
    }

    private static readonly int FftPow = (int)Math.Log(FftProps.FftLength, 2.0);

    public FftResult ToFft(float[] sample, int sampleRate)
    {
        var fftBuffer = new Complex[sample.Length];
        for (var i = 0; i < sample.Length; i++)
        {
            var value = sample[i];
            fftBuffer[i].X = (float)(value * FastFourierTransform.HammingWindow(i, sample.Length));
        }

        FastFourierTransform.FFT(true, FftPow, fftBuffer);
        var bands = CalculateBands(fftBuffer, sampleRate);

        return new FftResult() { Bands = bands };
    }

}
