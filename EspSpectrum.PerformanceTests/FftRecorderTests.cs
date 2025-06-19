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
public class FftRecorderTests
{
    private static IOptionsMonitor<DisplayConfig> GetOptionsMonitor()
    {
        var services = new ServiceCollection();
        services.Configure<SpectrumConfig>(c => { });
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IOptionsMonitor<DisplayConfig>>();
    }

    [Params(1, 4)]
    public int N { get; set; }

    [Benchmark(Baseline = true)]
    public void ReadSingleSine()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _);
        }
    }

    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBufferBigOverflow()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _);
        }
    }

    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBuffer()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(1);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _);
        }
    }

    //[Benchmark]
    public void ReadTwoHalves()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength / 2);
        audio.RecordSingleSine(FftProps.FftLength / 2);
        _ = recorder.TryReadSpectrum(out _);
    }

    //[Benchmark]
    public void ReadTwice()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = recorder.TryReadSpectrum(out _);
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = recorder.TryReadSpectrum(out _);
    }
}
