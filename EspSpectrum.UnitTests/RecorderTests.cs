using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Sounds;
using Microsoft.Extensions.Logging.Abstractions;

namespace EspSpectrum.UnitTests;

public sealed class RecorderTests : IDisposable
{
    private readonly FakeLoopbackWaveIn _fakeLoopbackWaveIn = new();
    private readonly FftRecorder recorder;

    public RecorderTests()
    {
        recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, _fakeLoopbackWaveIn);
    }

    [Fact]
    public async Task Test1()
    {
        var channels = _fakeLoopbackWaveIn.WaveFormat.Channels;
        var saw = new byte[FftProps.FftLength * 4 * channels];
        for (var i = 0; i < saw.Length; i += channels)
        {
            for (var c = 0; c < _fakeLoopbackWaveIn.WaveFormat.Channels; c++)
            {
                saw[i + c] = (byte)(i % 255);
            }
        }

        _fakeLoopbackWaveIn.FakeRecord(saw, saw.Length / _fakeLoopbackWaveIn.WaveFormat.Channels);
        var fft = await recorder.ReadFft();

        Assert.NotNull(fft);
        Assert.False(fft.Bands.All(b => b == 0));
    }

    public void Dispose()
    {
    }
}
