using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Sounds;
using Microsoft.Extensions.Logging.Abstractions;

namespace EspSpectrum.UnitTests;

public sealed class RecorderTests : IDisposable
{
    private readonly FakeLoopbackWaveIn _fakeLoopbackWaveIn = new();
    private readonly FftRecorder _recorder;

    public RecorderTests()
    {
        _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, _fakeLoopbackWaveIn);
    }

    [Fact]
    public async Task RecordSine440()
    {
        _fakeLoopbackWaveIn.RecordSingleSine();

        var fft = await _recorder.ReadFft();

        Assert.NotNull(fft);
        Assert.NotNull(fft.Bands);
        Assert.False(fft.Bands.All(b => b == 0));

        var peakIndex = fft.Bands
            .Select((value, index) => new { Value = value, Index = index })
            .MaxBy(g => g.Value)!
            .Index;
        Assert.True(FftProcessor.FrequencyBands.ElementAt(peakIndex) < Sine440.PeakFrequency);
        Assert.True(FftProcessor.FrequencyBands.ElementAt(peakIndex + 1) > Sine440.PeakFrequency);
    }

    public void Dispose()
    {
    }
}
