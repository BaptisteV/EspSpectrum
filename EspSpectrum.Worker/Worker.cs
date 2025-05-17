using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFftStream _stream;
    private readonly IWebsocketBars _ws;
    private readonly IWebsocketDisplay _wsDisplay;
    private DisplayConfig _conf;
    private readonly IOptionsMonitor<DisplayConfig> _confMonitor;

    public Worker(
        ILogger<Worker> logger,
        IFftStream stream,
        IWebsocketBars ws,
        IWebsocketDisplay wsDisplay,
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
                await _wsDisplay.Send(newConf);
                _conf = newConf;
            }
        });
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service");
        await _wsDisplay.Send(_confMonitor.CurrentValue);

        await ExecuteAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var bands in _stream.NextFft(stoppingToken))
        {
            await _ws.SendAudio(bands.Bands);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping service");
        await _ws.SendAudio(new int[FftProps.NBands]);
    }
}
