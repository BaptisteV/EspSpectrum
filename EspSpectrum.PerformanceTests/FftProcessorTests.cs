using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotMemory;
using BenchmarkDotNet.Diagnostics.dotTrace;
using EspSpectrum.Core.Fft;
using EspSpectrum.UnitTests.Utils;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[DisassemblyDiagnoser]
[DotMemoryDiagnoser]
[DotTraceDiagnoser]
public class FftProcessorTests
{
    private readonly FftProcessor _fftProcessor = new(Sine440.SampleRate);

    [Benchmark(Baseline = true)]
    public void ProcessNAudio()
    {
        _ = _fftProcessor.ToFft(Sine440.Buffer);
    }
}
