using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace EspSpectrum.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDisplayConfigWebsocket _wsDisplay;
    private DisplayConfig _conf;
    private readonly IOptionsMonitor<DisplayConfig> _confMonitor;
    private readonly IEspSpectrumRunner _stableSpectrumRunner;

    public Worker(
        ILogger<Worker> logger,
        IEspSpectrumRunner stableSpectrumReader,
        IDisplayConfigWebsocket wsDisplay,
        IOptionsMonitor<DisplayConfig> conf)
    {
        _logger = logger;
        _wsDisplay = wsDisplay;
        _confMonitor = conf;

        _conf = _confMonitor.CurrentValue;
        _confMonitor.OnChange(async (newConf) =>
        {
            if (newConf != _conf)
            {
                _logger.LogInformation("Updating display config");
                await _wsDisplay.SendDisplayConfig(newConf, CancellationToken.None);
                _conf = newConf;
            }
        });

        _stableSpectrumRunner = stableSpectrumReader;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service");

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        await _wsDisplay.SendDisplayConfig(_confMonitor.CurrentValue, cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing service...");

        _logger.LogInformation("First connection attempt...");
        var firstConnect = await _stableSpectrumRunner.TryConnectEsp(stoppingToken);
        _logger.LogInformation("First connection attempt done. {Status}", firstConnect ? "connected" : "NOT connected");
        while (!stoppingToken.IsCancellationRequested)
        {
            var loopResult = await _stableSpectrumRunner.Loop(stoppingToken);

            await HandleLoopResult(loopResult, stoppingToken);
        }
    }

    private async ValueTask HandleLoopResult(RunnerState loopResult, CancellationToken stoppingToken)
    {
        if (loopResult.HasFlag(RunnerState.ConnectedToEsp) && loopResult.HasFlag(RunnerState.LoopAudioCapture))
            return;

        if (!loopResult.HasFlag(RunnerState.ConnectedToEsp))
        {
            while (!await _stableSpectrumRunner.TryConnectEsp(stoppingToken))
            {
                var retryInterval = TimeSpan.FromSeconds(1);
                _logger.LogWarning("Not connected to ESP device, retrying in {RetryInterval}s ...", retryInterval.TotalSeconds);
                await Task.Delay(retryInterval, stoppingToken);
            }
        }

        if (!loopResult.HasFlag(RunnerState.LoopAudioCapture))
        {
            await _stableSpectrumRunner.StartAudio(stoppingToken);
        }
    }
}