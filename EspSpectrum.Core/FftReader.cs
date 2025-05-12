using Microsoft.Extensions.Logging;
using NAudio.Dsp;
using System.Collections.Concurrent;

namespace EspSpectrum.Core;

public class FftReader : IFftReader
{
    private readonly ILogger<FftReader> _logger;
    private static double[] _frequencyBands = [];
    private readonly IAudioRecorder _audioRecorder;
    private readonly ConcurrentQueue<double> _buffer = [];
    private static readonly int FftPow = (int)Math.Log(FftProps.FftLength, 2.0);

    public FftReader(IAudioRecorder audioRecorder, ILogger<FftReader> logger)
    {
        InitializeBandBoundaries();
        _audioRecorder = audioRecorder;
        _logger = logger;
    }

    private static void InitializeBandBoundaries()
    {
        _frequencyBands = new double[FftProps.NBands + 1];

        for (var i = 0; i < _frequencyBands.Length; i++)
        {
            var t = (float)i / (_frequencyBands.Length - 1);
            _frequencyBands[i] = FftProps.MinFreq * MathF.Pow(FftProps.MaxFreq / FftProps.MinFreq, t);
        }
    }

    private static int FrequencyToBin(double frequency, double binResolution)
    {
        return (int)Math.Round(Math.Clamp(frequency / binResolution, 0.0, FftProps.FftLength / 2.0 - 1.0));
    }

    public int[] CalculateBands(NAudio.Dsp.Complex[] fftResult)
    {
        var bandLevels = new int[FftProps.NBands];
        var binFrequencyResolution = (double)_audioRecorder.SampleRate / FftProps.FftLength;

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
            bandLevels[band] = (int)Math.Round(Math.Log10(bandEnergy + 1) * 20.0 * FftProps.ScaleFactor);
        }

        return bandLevels;
    }

    public int AvailableSamples() => _buffer.Count + _audioRecorder.RecordedSamples;

    public async ValueTask<FftResult> ReadLastFft(CancellationToken cancellation = default)
    {
        while (_buffer.Count < FftProps.FftLength)
        {
            await Task.Delay(FftProps.WaitForAudioTightLoop, cancellation);
            var newSamples = await _audioRecorder.ReadN(FftProps.FftLength - _buffer.Count);
            foreach (var sample in newSamples)
            {
                _buffer.Enqueue(sample);
            }
        }

        // Get samples for FFT processing
        var fftSamples = new double[FftProps.FftLength];

        // Remove ReadLength old samples
        for (int i = 0; i < FftProps.ReadLength; i++)
        {
            _buffer.TryDequeue(out fftSamples[i]);
        }

        // Copy remaining required samples from buffer
        var remainingSamples = _buffer.Take(FftProps.FftLength - FftProps.ReadLength).ToArray();
        Array.Copy(remainingSamples, 0, fftSamples, FftProps.ReadLength, remainingSamples.Length);

        return ToFft(fftSamples);
    }

    private FftResult ToFft(double[] sample)
    {
        var fftBuffer = new Complex[sample.Length];
        for (var i = 0; i < sample.Length; i++)
        {
            var value = sample[i];
            fftBuffer[i].X = (float)(value * FastFourierTransform.HammingWindow(i, FftProps.FftLength));
        }

        FastFourierTransform.FFT(true, FftPow, fftBuffer);
        var bands = CalculateBands(fftBuffer);

        return new FftResult() { Bands = bands };
    }
}
