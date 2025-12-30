using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Utils;

namespace EspSpectrum.PerformanceTests;

public class PartialDataReaderTests
{
    private PartialDataReader _dr = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _dr = new PartialDataReader(FftProps.FftLength, FftProps.ReadLength);
    }

    [Benchmark(Baseline = true)]

    public void PartialDataReaderTestSpan()
    {
        var a = sizeof(float) * Sine440.Buffer.Length;
        _dr.AddData(Sine440.Buffer);
        var buffer = new float[FftProps.FftLength];
#pragma warning disable S108 // Nested blocks of code should not be left empty
        while (_dr.TryReadAudioFrame(buffer)) { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
    }
}
