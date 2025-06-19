using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDisplayConfigWebsocket _wsDisplay;
    private DisplayConfig _conf;
    private readonly IOptionsMonitor<DisplayConfig> _confMonitor;
    private readonly IStableSpectrumReader _stableSpectrumReader;

    public Worker(
        ILogger<Worker> logger,
        IStableSpectrumReader stableSpectrumReader,
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
                await _wsDisplay.SendDisplayConfig(newConf);
                _conf = newConf;
            }
        });

        _stableSpectrumReader = stableSpectrumReader;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service");

        await _wsDisplay.SendDisplayConfig(_confMonitor.CurrentValue);

        _stableSpectrumReader.Start();

        await ExecuteAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _stableSpectrumReader.Tick(stoppingToken);
        }
    }
}
