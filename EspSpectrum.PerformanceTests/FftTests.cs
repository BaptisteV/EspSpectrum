using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FftTests
{
    private FakeLoopbackWaveIn _fakeLoopbackWaveIn;
    private FftRecorder _recorder;

    [GlobalSetup]
    public void Setup()
    {
        _fakeLoopbackWaveIn = new FakeLoopbackWaveIn();
        var services = new ServiceCollection();
        services.Configure<SpectrumConfig>(c => { });
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<DisplayConfig>>();

        _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, _fakeLoopbackWaveIn, optionsMonitor);
    }

    [Benchmark]
    public async Task ReadSingleSine()
    {
        _recorder.Start();
        _fakeLoopbackWaveIn.RecordSingleSine();
        _ = await _recorder.ReadFft();
    }
}
