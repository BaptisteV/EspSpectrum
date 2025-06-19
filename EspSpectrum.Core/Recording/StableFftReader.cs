using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class StableSpectrumReader(
    IOptionsMonitor<DisplayConfig> displayConfigMonitor,
    ISyncSpectrumReader spectrumReader,
    ISpectrumWebsocket ws,
    ILogger<StableSpectrumReader> logger) : IStableSpectrumReader
{
    private readonly Stopwatch _sw = new();

    private async ValueTask CalculateAndSend(CancellationToken cancellationToken)
    {
        var spectrum = spectrumReader.GetLatestBlocking(cancellationToken);
        await ws.SendSpectrum(spectrum.Bands);
    }

    public async ValueTask Tick(CancellationToken cancellationToken)
    {
        _sw.Restart();
        var targetRate = displayConfigMonitor.CurrentValue.SendInterval;

        await CalculateAndSend(cancellationToken);

        var remaining = targetRate - _sw.Elapsed;

        var late = remaining < TimeSpan.Zero;
        if (late)
        {
            logger.LogWarning("{Elapsed}ms late",
                remaining.TotalMilliseconds);
            return;
        }

        if (remaining.TotalMilliseconds < 0.5)
            return;

        PreciseSleep.Wait(remaining);
    }

    public void Start()
    {
        spectrumReader.Start();
    }
}

