using EspSpectrum.Core;
using EspSpectrum.Core.Display;

namespace EspSpectrum.Worker;

public class Worker(ILogger<Worker> logger, IFftStream stream, IEspWebsocket ws, IDisplayConfigChangeHandler dcch) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IFftStream _stream = stream;
    private readonly IEspWebsocket _ws = ws;
    private readonly IDisplayConfigChangeHandler _dcch = dcch;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var bands in _stream.NextFft(stoppingToken))
        {
            await _ws.SendAudio(bands.Bands);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service");
        await ExecuteAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping service");
        await _ws.SendAudio(new int[FftProps.NBands]);
    }
}
