using BenchmarkDotNet.Attributes;
using EspSpectrum.Core;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FftRecorderTests
{
    IServiceProvider _serviceProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();

        var myConfiguration = new Dictionary<string, string?>
        {
            {"Key1", "Value1"},
            {"Nested:Key1", "NestedValue1"},
            {"Nested:Key2", "NestedValue2"}
        };

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(myConfiguration).Build();
        services.AddCoreServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Params(1, 4)]
    public int N { get; set; }

    [Benchmark(Baseline = true)]
    public void ReadSingleSine()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = _serviceProvider.GetRequiredService<IFftRecorder>();
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
        }
    }

    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBufferBigOverflow()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = _serviceProvider.GetRequiredService<IFftRecorder>();
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(FftProps.FftLength);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
        }
    }

    //[Benchmark(Baseline = true)]
    public void ReadSingleSineFullBuffer()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = _serviceProvider.GetRequiredService<IFftRecorder>();
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        // This one should overflow and get ignored (PartialDataReader._maxQueueSize = FftProps.FftLength * 2)
        audio.RecordSingleSine(1);
        for (var i = 0; i < N; i++)
        {
            audio.RecordSingleSine(FftProps.ReadLength);
            _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
        }
    }

    //[Benchmark]
    public void ReadTwoHalves()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = _serviceProvider.GetRequiredService<IFftRecorder>();
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength / 2);
        audio.RecordSingleSine(FftProps.FftLength / 2);
        _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
    }

    //[Benchmark]
    public void ReadTwice()
    {
        var audio = new FakeLoopbackWaveIn();
        var recorder = _serviceProvider.GetRequiredService<IFftRecorder>();
        recorder.Start();
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
        audio.RecordSingleSine(FftProps.FftLength);
        audio.RecordSingleSine(FftProps.FftLength);
        _ = recorder.TryReadSpectrum(out _, CancellationToken.None);
    }
}
