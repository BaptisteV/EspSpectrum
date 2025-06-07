using EspSpectrum.Core.Fft;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace EspSpectrum.PerformanceTests;

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

    public void RecordSingleSine(int nSample = FftProps.FftLength) => RecordSingleSineSpan2(nSample);
    /*
{
    var s = Sine440.Buffer.Take(nSample).ToArray();

    var sine = new List<byte>(nSample * 4 * WaveFormat.Channels);
    for (var iBuff = 0; iBuff < s.Length; iBuff++)
    {
        var chunck = BitConverter.GetBytes(s[iBuff]);
        for (var c = 0; c < WaveFormat.Channels; c++)
        {
            sine.AddRange(chunck);
        }
    }
    DataAvailable?.Invoke(this, new WaveInEventArgs([.. sine], sine.Count));
}*/

    public void RecordSingleSineSpan(int nSample = FftProps.FftLength)
    {
        ReadOnlySpan<float> sineSamples = Sine440.Buffer.Take(nSample).ToArray(); // unavoidable unless Sine440.Buffer is already a Span
        int sampleSize = 4; // size of float
        int frameSize = sampleSize * WaveFormat.Channels;

        // Preallocate the exact buffer size
        byte[] buffer = new byte[sineSamples.Length * frameSize];
        Span<byte> bufferSpan = buffer;

        for (int i = 0; i < sineSamples.Length; i++)
        {
            float sample = sineSamples[i];

            // Convert float to bytes once per sample
            Span<byte> sampleBytes = stackalloc byte[sampleSize];
            MemoryMarshal.Write(sampleBytes, ref sample);

            // Write same sample to each channel
            for (int ch = 0; ch < WaveFormat.Channels; ch++)
            {
                int offset = i * frameSize + ch * sampleSize;
                sampleBytes.CopyTo(bufferSpan.Slice(offset, sampleSize));
            }
        }

        // Fire the event with the filled buffer
        DataAvailable?.Invoke(this, new WaveInEventArgs(buffer, buffer.Length));
    }


    public void RecordSingleSineSpan2(int nSample = FftProps.FftLength)
    {
        ReadOnlySpan<float> sineSamples = Sine440.Buffer.AsSpan().Slice(0, nSample); // unavoidable unless Sine440.Buffer is already a Span
        int frameSize = 4 * WaveFormat.Channels;

        Span<byte> bufferSpan = stackalloc byte[sineSamples.Length * frameSize];

        for (int i = 0; i < sineSamples.Length; i++)
        {
            float sample = sineSamples[i];

            // Convert float to bytes once per sample
            Span<byte> sampleBytes = stackalloc byte[4];
            MemoryMarshal.Write(sampleBytes, ref sample);

            // Write same sample to each channel
            for (int ch = 0; ch < WaveFormat.Channels; ch++)
            {
                int offset = i * frameSize + ch * 4;
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
