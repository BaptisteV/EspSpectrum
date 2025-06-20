using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.UnitTests.Utils;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class FftProcessorTests
{
    private readonly FftProcessor _fftProcessor = new(Sine440.SampleRate);

    [Benchmark(Baseline = true)]
    public void ProcessNAudio()
    {
        _ = _fftProcessor.ToFft(Sine440.Buffer);
    }
}
