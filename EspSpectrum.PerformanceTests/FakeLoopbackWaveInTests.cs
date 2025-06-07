using BenchmarkDotNet.Attributes;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FakeLoopbackWaveInTests
{
    [Params(1024, 2048, 4096, 4096 * 2)]
    public int NSample { get; set; }

    [Benchmark]
    public void Record()
    {
        using var waveIn = new FakeLoopbackWaveIn();
        waveIn.RecordSingleSine(NSample);
    }

    [Benchmark(Baseline = true)]
    public void RecordSpan()
    {
        using var waveIn = new FakeLoopbackWaveIn();
        waveIn.RecordSingleSineSpan(NSample);
    }

    [Benchmark]
    public void RecordSpan2()
    {
        using var waveIn = new FakeLoopbackWaveIn();
        waveIn.RecordSingleSineSpan2(NSample);
    }
}
