using EspSpectrum.Core;

namespace EspSpectrum.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFftStream _stream;
        private readonly IEspWebsocket _ws;

        public Worker(ILogger<Worker> logger, IFftStream stream, IEspWebsocket ws)
        {
            _logger = logger;
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
    }
}
