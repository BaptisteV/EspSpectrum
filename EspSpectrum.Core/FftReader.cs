using Microsoft.Extensions.Logging;
using NAudio.Dsp;
using System.Collections.Concurrent;

namespace EspSpectrum.Core;

public class FftReader : IFftReader
{
    private readonly ILogger<FftReader> _logger;
    private static float[] _frequencyBands = [];
    private readonly IAudioRecorder _audioRecorder;
    private readonly ConcurrentQueue<float> _buffer = [];

    public FftReader(IAudioRecorder audioRecorder, ILogger<FftReader> logger)
    {
        InitializeBandBoundaries();
        _audioRecorder = audioRecorder;
        _logger = logger;
    }

    private static void InitializeBandBoundaries()
    {
        _frequencyBands = new float[BandsConfig.NBands + 1];

        for (var i = 0; i < _frequencyBands.Length; i++)
        {
            var t = (float)i / (_frequencyBands.Length - 1);
            _frequencyBands[i] = BandsConfig.minFreq * MathF.Pow(BandsConfig.maxFreq / BandsConfig.minFreq, t);
        }
    }

    private static int FrequencyToBin(float frequency, float binResolution)
    {
        return Math.Clamp((int)(frequency / binResolution), 0, BandsConfig.FftLength / 2 - 1);
    }

    public int[] CalculateBands(NAudio.Dsp.Complex[] fftResult)
    {
        var bandLevels = new float[BandsConfig.NBands];
        var binFrequencyResolution = (float)_audioRecorder.SampleRate / BandsConfig.FftLength;

        for (var band = 0; band < BandsConfig.NBands; band++)
        {
            // Find the FFT bins corresponding to this band's frequency range
            var startBin = FrequencyToBin(_frequencyBands[band], binFrequencyResolution);
            var endBin = FrequencyToBin(_frequencyBands[band + 1], binFrequencyResolution);

            // Calculate band energy
            var bandEnergy = 0f;
            for (var bin = startBin; bin < endBin; bin++)
            {
                // Calculate magnitude (energy) of the complex FFT result
                bandEnergy += (float)Math.Sqrt(
                    fftResult[bin].X * fftResult[bin].X +
                    fftResult[bin].Y * fftResult[bin].Y
                );
            }

            // Apply logarithmic scaling
            bandLevels[band] = (float)Math.Log10(bandEnergy + BandsConfig.AddFactor) * 20.0f * BandsConfig.ScaleFactor;
        }

        return [.. bandLevels.Select(b => (int)Math.Round(b))];
    }

    public int AvailableSamples() => _buffer.Count + _audioRecorder.RecordedSamples;

    public async Task<FftResult> ReadLastFft()
    {
        while (_buffer.Count < BandsConfig.FftLength)
        {
            var sample = (await _audioRecorder.ReadN(BandsConfig.ReadLength)).ToList();
            sample.ForEach(_buffer.Enqueue);
            await Task.Delay(BandsConfig.WaitForAudioTightLoop);
        }

        var result = new List<float>();
        var i = 0;
        while (i < BandsConfig.ReadLength)
        {
            _buffer.TryDequeue(out var value);
            result.Add(value);
            i++;
        }
        var bCopy = _buffer.ToArray();
        for (var j = 0; j < BandsConfig.FftLength - BandsConfig.ReadLength; j++)
        {
            result.Add(bCopy[j]);
        }
        return ToFft([.. result]);
    }

    private FftResult ToFft(float[] sample)
    {
        var fftBuffer = new Complex[sample.Length];
        for (var i = 0; i < sample.Length; i++)
        {
            var value = sample[i];
            fftBuffer[i].X = (float)(value * FastFourierTransform.HammingWindow(i, BandsConfig.FftLength));
        }

        FastFourierTransform.FFT(true, (int)Math.Log(BandsConfig.FftLength, 2.0), fftBuffer);
        var bands = CalculateBands(fftBuffer);

        return new FftResult() { Bands = bands };
    }
}
