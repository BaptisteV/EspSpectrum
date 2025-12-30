using BenchmarkDotNet.Attributes;
using EspSpectrum.Core;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using EspSpectrum.UnitTests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;

namespace EspSpectrum.PerformanceTests;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[DisassemblyDiagnoser]
public class EspSpectrumRunnerTests
{
    private IEspSpectrumRunner _spectrumRunner = null!;
    private FakeLoopbackWaveIn _fakeLoopbackWaveIn = null!;

    [Params(1, 4, 10, 20)]
    public int TickInterval { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();

        var configuration = new ConfigurationBuilder().Build();
        services.AddCoreServices(configuration);
        _fakeLoopbackWaveIn = new FakeLoopbackWaveIn();
        services.AddTransient<IWaveIn>(_ => _fakeLoopbackWaveIn);

        services.Configure<DisplayConfig>(c =>
        {
            c.SendInterval = TimeSpan.FromMilliseconds(TickInterval);
        });
        services.AddSingleton<IDisplayConfigWebsocket, FakeEspWebsocket>();
        services.AddSingleton<ISpectrumWebsocket, FakeEspWebsocket>();
        var sp = services.BuildServiceProvider();
        _spectrumRunner = sp.GetRequiredService<IEspSpectrumRunner>();
    }

    [Benchmark]
    public async Task EspSpectrumTick()
    {
        await _spectrumRunner.Start();
        _fakeLoopbackWaveIn.RecordSingleSine();
        await _spectrumRunner.Loop(CancellationToken.None);
    }
}