using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FftProcessorTests
{
    private FftProcessor _fftProcessor => new(Sine440.SampleRate);

    [Benchmark]
    public async Task ProcessSine()
    {
        _ = _fftProcessor.ToFft(Sine440.Buffer);
    }
}
