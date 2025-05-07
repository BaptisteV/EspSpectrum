using EspSpectrum.Core;

namespace EspSpectrum.Worker;

public class Worker : BackgroundService
{
    private readonly IFftStream _stream;
    private readonly IEspWebsocket _ws;

    public Worker(IFftStream stream, IEspWebsocket ws)
    {
        _stream = stream;
        _ws = ws;
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
        await _ws.SendAudio(new int[BandsConfig.NBands]);
    }
}
