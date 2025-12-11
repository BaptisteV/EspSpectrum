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
    private readonly ITickTimingMonitor timingMonitor;
    private readonly ILogger<EspSpectrumRunner> logger;
    private TimeSpan _nextTickMilliseconds = TimeSpan.Zero;
    private bool _started = false;

    public EspSpectrumRunner(
        IOptionsMonitor<DisplayConfig> displayConfigMonitor,
        ISyncSpectrumReader spectrumReader,
        ISpectrumWebsocket ws,
        ITickTimingMonitor timingMonitor,
        ILogger<EspSpectrumRunner> logger)
    {
        this.spectrumReader = spectrumReader;
        this.ws = ws;
        this.timingMonitor = timingMonitor;
        this._interval = displayConfigMonitor.CurrentValue.SendInterval;
        this.logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    public async ValueTask DoFftAndSend(CancellationToken cancellationToken)
    {
        var spectrum = spectrumReader.GetLatestBlocking(cancellationToken);
        await ws.SendSpectrum(spectrum);
    }

    public bool WaitForNextTick(CancellationToken cancellationToken)
    {
        if (_started)
        {
            _nextTickMilliseconds = _stopwatch.Elapsed + _interval;
            _started = false;
            return true; // immediate first tick
        }

        var delay = _nextTickMilliseconds - _stopwatch.Elapsed;
        _nextTickMilliseconds += _interval;

        timingMonitor.NotifyTickDiff(delay);

        // Pretty much no overrun with / 4
        return WaitIfNecessary(delay / 4, cancellationToken);
    }

    private bool WaitIfNecessary(TimeSpan delay, CancellationToken cancellationToken)
    {
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
            logger.LogInformation("Overrun: {Delay:n2}ms", delay.TotalMilliseconds);
            // Overrun: we are already late
            // Optionally log or handle
        }
        return !cancellationToken.IsCancellationRequested;
    }

    public void Start()
    {
        spectrumReader.Start();
        timingMonitor.StartInBg();
    }
}

