using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.PerformanceTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public sealed class RecorderTests : BaseTests, IDisposable
{
    private readonly FakeLoopbackWaveIn _fakeLoopbackWaveIn = new();
    private readonly FftRecorderSpan _recorder;
    private readonly CancellationTokenSource _cts = new();


    public RecorderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var services = new ServiceCollection();
        services.Configure<SpectrumConfig>(c => { });
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<DisplayConfig>>();

        _recorder = new FftRecorderSpan(LoggerFactory.CreateLogger<FftRecorderSpan>(), _fakeLoopbackWaveIn, optionsMonitor);
        _recorder.Start();
    }

    [Fact]
    public void ReadSine440()
    {
        _fakeLoopbackWaveIn.RecordSingleSine();

        _ = _recorder.TryReadSpectrum(out var fft);

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

    [Fact]
    public void ReadSine440Twice()
    {
        _fakeLoopbackWaveIn.RecordSingleSine();
        _fakeLoopbackWaveIn.RecordSingleSine();

        _ = _recorder.TryReadSpectrum(out var fft);

        Assert.NotNull(fft);
        Assert.NotNull(fft.Bands);
        Assert.False(fft.Bands.All(b => b == 0));
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}
