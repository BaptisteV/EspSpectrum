using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class EspSpectrumRunner(
    IOptionsMonitor<DisplayConfig> displayConfigMonitor,
    ISyncSpectrumReader spectrumReader,
    ISpectrumWebsocket ws,
    ILogger<EspSpectrumRunner> logger) : IEspSpectrumRunner
{
    private readonly Stopwatch _sw = new();

    private async ValueTask CalculateAndSend(CancellationToken cancellationToken)
    {
        var spectrum = spectrumReader.GetLatestBlocking(cancellationToken);
        await ws.SendSpectrum(spectrum);
    }

    public async ValueTask Tick(CancellationToken cancellationToken)
    {
        _sw.Restart();
        var targetRate = displayConfigMonitor.CurrentValue.SendInterval;

        await CalculateAndSend(cancellationToken);

        var remaining = targetRate - _sw.Elapsed;

        if (remaining.TotalMilliseconds <= 0.5)
        {
            if (remaining < TimeSpan.Zero)
                logger.LogWarning("{Elapsed}ms late", remaining.TotalMilliseconds);
            return;
        }

        PreciseSleep.Wait(remaining, cancellationToken);
    }

    public void Start()
    {
        spectrumReader.Start();
    }
}

