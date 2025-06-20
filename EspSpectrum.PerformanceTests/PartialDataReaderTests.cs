using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Utils;
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
        _dr.AddData(Sine440.Buffer);
        float[] data;
        while (_dr.TryReadAudioFrame(out data)) { }
    }

    [Benchmark]

    public void PartialDataReaderTestSpan()
    {
        _dr.AddData(Sine440.Buffer);
        Span<float> buffer = stackalloc float[FftProps.FftLength];
        while (_dr.TryReadAudioFrame(buffer)) { }
    }
}
