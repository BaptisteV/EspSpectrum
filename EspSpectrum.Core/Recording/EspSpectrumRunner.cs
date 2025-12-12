using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class EspSpectrumRunner : IEspSpectrumRunner
{
    private readonly TimeSpan _interval;
    private readonly Stopwatch _stopwatch;
    private readonly ISyncSpectrumReader _spectrumReader;
    private readonly ISpectrumWebsocket _ws;
    private readonly ITickTimingMonitor _timingMonitor;
    private readonly IPreciseSleep _sleep;
    private readonly ILogger<EspSpectrumRunner> _logger;
    private TimeSpan _nextTickMilliseconds = TimeSpan.Zero;
    private bool _started = false;

    public EspSpectrumRunner(
        IOptionsMonitor<DisplayConfig> displayConfigMonitor,
        ISyncSpectrumReader spectrumReader,
        ISpectrumWebsocket ws,
        ITickTimingMonitor timingMonitor,
        IPreciseSleep sleep,
        ILogger<EspSpectrumRunner> logger)
    {
        _spectrumReader = spectrumReader;
        _ws = ws;
        _timingMonitor = timingMonitor;
        _sleep = sleep;
        _interval = displayConfigMonitor.CurrentValue.SendInterval;
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
    }

    public async ValueTask DoFftAndSend(CancellationToken cancellationToken)
    {
        var spectrum = _spectrumReader.GetLatestBlocking(cancellationToken);
        await _ws.SendSpectrum(spectrum);
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

        _timingMonitor.NotifyTickDiff(delay);

        // Pretty much no overrun with / 4
        return WaitIfNecessary(delay / 4, cancellationToken);
    }

    private bool WaitIfNecessary(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.FromMilliseconds(0.1))
        {
            try
            {
                _logger.LogTrace("Waiting for: {Delay:n2}ms", delay.TotalMilliseconds);
                _sleep.Wait(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
        else
        {
            _logger.LogInformation("Overrun: {Delay:n2}ms", delay.TotalMilliseconds);
            // Overrun: we are already late
            // Optionally log or handle
        }
        return !cancellationToken.IsCancellationRequested;
    }

    public void Start()
    {
        _spectrumReader.Start();
        _timingMonitor.StartInBg();
    }
}

