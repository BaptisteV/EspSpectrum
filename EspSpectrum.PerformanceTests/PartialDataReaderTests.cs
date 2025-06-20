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
    private PartialDataReader _dr = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dr = new PartialDataReader(NullLogger<PartialDataReader>.Instance, FftProps.FftLength, FftProps.ReadLength);
    }

    [Benchmark(Baseline = true)]
    public void PartialDataReaderTestArray()
    {
        _dr.AddData(Sine440.Buffer);
        var datar = new float[FftProps.FftLength];
#pragma warning disable S108 // Nested blocks of code should not be left empty
        while (_dr.TryReadAudioFrame(datar)) { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
    }

    [Benchmark]

    public void PartialDataReaderTestSpan()
    {
        _dr.AddData(Sine440.Buffer);
        var buffer = new float[FftProps.FftLength];
#pragma warning disable S108 // Nested blocks of code should not be left empty
        while (_dr.TryReadAudioFrame(buffer)) { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
    }
}
