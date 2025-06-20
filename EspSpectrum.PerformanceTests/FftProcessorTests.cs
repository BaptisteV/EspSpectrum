using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.UnitTests.Utils;

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
