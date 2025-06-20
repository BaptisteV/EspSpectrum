using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FftProcessorTests
{
    [Benchmark]
    public void ProcessSine()
    {
        var fftProcessor = new FftProcessor(Sine440.SampleRate);
        _ = fftProcessor.ToFft(Sine440.Buffer);
    }
}
