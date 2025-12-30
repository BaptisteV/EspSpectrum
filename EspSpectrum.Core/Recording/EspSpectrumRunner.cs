using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public sealed class EspSpectrumRunner : IEspSpectrumRunner
{
    private readonly TimeSpan _sendInterval;
    private readonly Stopwatch _stopwatch = new();
    private readonly ISyncSpectrumReader _spectrumReader;
    private readonly ISpectrumWebsocket _ws;
    private readonly ITickTimingMonitor _timingMonitor;
    private readonly IPreciseSleep _sleep;
    private readonly ILogger<EspSpectrumRunner> _logger;

    private readonly CancellationTokenSource _cts = new();
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
        _sendInterval = displayConfigMonitor.CurrentValue.SendInterval;
        _logger = logger;

    }

    public async ValueTask DoFftAndSend(CancellationToken cancellationToken)
    {
        var spectrum = await _spectrumReader.GetLatestBlocking(cancellationToken);
        await _ws.SendSpectrum(spectrum);
    }

    private bool _started = false;

    public async Task Start()
    {
        if (_started)
            return;
        _started = true;

        var connected = false;
        try
        {
            connected = await _ws.TryConnect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to ESP WebSocket at start");
            connected = false;
        }

        if (!connected)
        {
            _logger.LogWarning("Could not connect to ESP WebSocket at start");
        }
        await _ws.TryConnectLoop();
        await _timingMonitor.LogSummaryLoop();

        _stopwatch.Start();
        _spectrumReader.Start();

        _ = Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                await Loop(_cts.Token);
            }
        }, _cts.Token);
    }

    public async Task Loop(CancellationToken cancellationToken)
    {
        _stopwatch.Restart();
        await DoFftAndSend(cancellationToken);
        _timingMonitor.NotifyFFTSent(DateTimeOffset.UtcNow);

        var elapsed = _stopwatch.Elapsed;
        if (elapsed > _sendInterval)
        {
            _logger.LogWarning("Processing took longer ({Elapsed}ms) than the send interval ({SendInterval}ms)", elapsed.TotalMilliseconds, _sendInterval.TotalMilliseconds);
        }
        else
        {
            _logger.LogTrace("Processing took {Elapsed}ms, sleeping for {Sleep}ms", elapsed.TotalMilliseconds, (_sendInterval - elapsed).TotalMilliseconds);
            await _sleep.Wait(_sendInterval - elapsed, cancellationToken);
        }
    }

    public void Subscribe(SpectrumObserver observer)
    {
        _spectrumReader.Subscribe(observer);
    }

    public async Task Stop()
    {
        await _cts.CancelAsync();
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}

