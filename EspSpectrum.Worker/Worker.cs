using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISpectrumStream _stream;
    private readonly ISpectrumWebsocket _ws;
    private readonly IDisplayConfigWebsocket _wsDisplay;
    private DisplayConfig _conf;
    private readonly IOptionsMonitor<DisplayConfig> _confMonitor;

    public Worker(
        ILogger<Worker> logger,
        ISpectrumStream stream,
        ISpectrumWebsocket ws,
        IDisplayConfigWebsocket wsDisplay,
        IOptionsMonitor<DisplayConfig> conf)
    {
        _logger = logger;
        _stream = stream;
        _ws = ws;
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
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service");
        await _wsDisplay.SendDisplayConfig(_confMonitor.CurrentValue);
        _stream.Start();
        await ExecuteAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var bands in _stream.NextFft(stoppingToken))
        {
            await _ws.SendSpectrum(bands.Bands);
        }
        _logger.LogInformation("Service interrupted");
    }
}
