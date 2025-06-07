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
        var _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }

    [Benchmark]
    public void ReadSingleSineSpan()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }


    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBufferBigOverflow()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }

    //[Benchmark]
    public void ReadSingleSineFullBufferBigOverflowSpan()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }

    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBuffer()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(1);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }

    //[Benchmark]
    public void ReadSingleSineFullBufferSpan()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorderSpan(NullLogger<FftRecorderSpan>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(1);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = _recorder.TryReadFft();
        }
    }
    /*
    [Benchmark]
    public async Task ReadTwoHalves()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength / 2);
        audio.RecordSingleSine(FftProps.FftLength / 2);
        _ = await _recorder.ReadFft();
    }

    [Benchmark]
    public async Task ReadTwice()
    {
        var audio = new FakeLoopbackWaveIn();
        var _recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, audio, GetOptionsMonitor());
        _recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = await _recorder.ReadFft();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = await _recorder.ReadFft();
    }*/
}
