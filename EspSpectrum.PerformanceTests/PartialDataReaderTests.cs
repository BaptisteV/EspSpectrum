using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging.Abstractions;

namespace EspSpectrum.PerformanceTests;


[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class PartialDataReaderTests
{
    private PartialDataReader _dr;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dr = new PartialDataReader(NullLogger<PartialDataReader>.Instance, FftProps.FftLength, FftProps.ReadLength);
    }

    [Benchmark(Baseline = true)]
    public void PartialDataReaderTestArray()
    {
        //var dataChannel = Channel.CreateUnbounded<float>();
        //await FeederThread.FeedData(dataChannel, FftProps.FftLength);
        _dr.AddData(Sine440.Buffer);
        while (_dr.TryReadAudioFrame(out _))
        { }
    }
    [Benchmark]

    public void PartialDataReaderTestSpan()
    {
        //var dataChannel = Channel.CreateUnbounded<float>();
        //await FeederThread.FeedData(dataChannel, FftProps.FftLength);
        _dr.AddData(Sine440.Buffer);
        Span<float> buffer = stackalloc float[FftProps.FftLength];
        while (_dr.TryReadAudioFrame(buffer)) { }
    }
}
