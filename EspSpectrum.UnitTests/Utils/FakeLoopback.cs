using EspSpectrum.Core.Fft;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace EspSpectrum.UnitTests.Utils;

public sealed class FakeLoopbackWaveIn : IWaveIn
{
    public WaveFormat WaveFormat { get; set; } = new WaveFormat(44100, 32, 2);

    public event EventHandler<WaveInEventArgs>? DataAvailable;
    public event EventHandler<StoppedEventArgs>? RecordingStopped;
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Dispose();
    }

    public void RecordSingleSine(int nSample = FftProps.FftLength)
    {
        ReadOnlySpan<float> sineSamples = Sine440.Buffer.AsSpan(0, nSample);
        var frameSize = 4 * WaveFormat.Channels;

        Span<byte> bufferSpan = stackalloc byte[sineSamples.Length * frameSize];
        Span<byte> sampleBytes = stackalloc byte[4];
        for (var i = 0; i < sineSamples.Length; i++)
        {
            var sample = sineSamples[i];
            MemoryMarshal.Write(sampleBytes, in sample);

            // Write same sample to each channel
            for (var ch = 0; ch < WaveFormat.Channels; ch++)
            {
                var offset = i * frameSize + ch * 4;
                sampleBytes.CopyTo(bufferSpan.Slice(offset, 4));
            }
        }

        DataAvailable?.Invoke(this, new WaveInEventArgs(bufferSpan.ToArray(), bufferSpan.Length));
    }

    public void StartRecording()
    {
    }

    public void StopRecording()
    {
        _cts.Cancel();
        RecordingStopped?.Invoke(this, new StoppedEventArgs());
    }
}
