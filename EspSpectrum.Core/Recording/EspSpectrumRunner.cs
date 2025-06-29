using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class EspSpectrumRunner : IEspSpectrumRunner
{
    private readonly TimeSpan _interval;
    private readonly Stopwatch _stopwatch;
    private readonly ISyncSpectrumReader spectrumReader;
    private readonly ISpectrumWebsocket ws;
    private readonly ILogger<EspSpectrumRunner> logger;
    private TimeSpan _nextTickMilliseconds = TimeSpan.Zero;
    private bool _started = false;

    public EspSpectrumRunner(
        IOptionsMonitor<DisplayConfig> displayConfigMonitor,
        ISyncSpectrumReader spectrumReader,
        ISpectrumWebsocket ws,
        ILogger<EspSpectrumRunner> logger)
    {
        this.spectrumReader = spectrumReader;
        this.ws = ws;
        this.logger = logger;
        _interval = displayConfigMonitor.CurrentValue.SendInterval;
        _stopwatch = Stopwatch.StartNew();
    }

    public async ValueTask DoFftAndSend(CancellationToken cancellationToken)
    {
        var spectrum = spectrumReader.GetLatestBlocking(cancellationToken);
        await ws.SendSpectrum(spectrum);
    }

    public bool WaitForNextTick(CancellationToken cancellationToken)
    {
        if (!_started)
        {
            _nextTickMilliseconds = _stopwatch.Elapsed + _interval;
            _started = true;
            return true; // immediate first tick
        }

        var elapsed = _stopwatch.Elapsed;
        var delay = _nextTickMilliseconds - elapsed;
        _nextTickMilliseconds += _interval;
        if (delay > TimeSpan.FromMilliseconds(0.1))
        {
            try
            {
                logger.LogTrace("Waiting for: {Delay:n2}ms", delay.TotalMilliseconds);
                PreciseSleep.Wait(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
        else
        {
            logger.LogWarning("Overrun: {Delay:n2}ms", delay.TotalMilliseconds);
            // Overrun: we are already late
            // Optionally log or handle
        }
        return !cancellationToken.IsCancellationRequested;
    }

    public void Start()
    {
        spectrumReader.Start();
    }
}

