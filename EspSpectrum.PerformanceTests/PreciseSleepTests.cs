using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[DisassemblyDiagnoser]
public class PreciseSleepTests
{
    private PreciseSleep sleep;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        sleep = new(services.BuildServiceProvider().GetRequiredService<ILogger<PreciseSleep>>());
    }

    [Params(0.5, 1.0, 5.0, 10.0, 20.0, 50.0, 100.0)]
    public double Ms { get; set; }

    [Benchmark(Baseline = true)]
    public async Task PreciseSleep_Wait_1ms()
    {
        var ticksBefore = Stopwatch.GetTimestamp();
        await sleep.Wait(TimeSpan.FromMilliseconds(Ms), CancellationToken.None);
        var ticksAfter = Stopwatch.GetTimestamp();
        var elapsedMs = (ticksAfter - ticksBefore) * 1000.0 / Stopwatch.Frequency;
    }

}
