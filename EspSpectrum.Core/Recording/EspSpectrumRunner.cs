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
    private RunnerState _state;
    private readonly TimeSpan _sendInterval;
    private readonly ISyncSpectrumReader _spectrumReader;
    private readonly ISpectrumWebsocket _ws;
    private readonly ITickTimingMonitor _timingMonitor;
    private readonly IPreciseSleep _sleep;
    private readonly HashSet<SpectrumObserver> _observers = [];
    private readonly ILogger<EspSpectrumRunner> _logger;

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

    public async Task StartAudio(CancellationToken cancellationToken)
    {
        StartAudioCapture(cancellationToken);

        Subscribe(new SpectrumObserver(_logger, async spectrum =>
        {
            await _ws.SendSpectrum(spectrum, cancellationToken);
            _timingMonitor.NotifyFFTSent(DateTimeOffset.UtcNow);
        }));
        await _timingMonitor.LogSummaryLoop();
    }

    public async Task<bool> TryConnectEsp(CancellationToken cancellationToken)
    {
        var connected = await _ws.TryConnect(cancellationToken);
        if (connected)
        {
            _state |= RunnerState.ConnectedToEsp;
        }
        else
        {
            _state &= ~RunnerState.ConnectedToEsp;
        }
        return connected;
    }

    private void StartAudioCapture(CancellationToken cancellationToken)
    {
        _spectrumReader.Start();
        _state |= RunnerState.LoopAudioCapture;
    }

    private readonly Stopwatch _sw = new();

    public async Task<RunnerState> Loop(CancellationToken cancellationToken)
    {
        if (!_state.HasFlag(RunnerState.LoopAudioCapture))
            return _state;

        _sw.Restart();
        var spectrum = await _spectrumReader.ReadBlocking(cancellationToken);
        await Task.WhenAll(_observers.Select(observer => observer.OnNext(spectrum).AsTask()));
        var elapsed = _sw.Elapsed;
        if (elapsed > _sendInterval)
        {
            _logger.LogTrace("Processing took longer ({Elapsed}ms) than the send interval ({SendInterval}ms)", elapsed.TotalMilliseconds, _sendInterval.TotalMilliseconds);
        }
        else
        {
            _logger.LogTrace("Processing took {Elapsed}ms, sleeping for {Sleep}ms", elapsed.TotalMilliseconds, (_sendInterval - elapsed).TotalMilliseconds);
            await _sleep.Wait(_sendInterval - elapsed, cancellationToken);
        }

        return _state;
    }

    public void Subscribe(SpectrumObserver observer)
    {
        var added = _observers.Add(observer);
        if (!added)
        {
            throw new InvalidOperationException("Observer already subscribed");
        }
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }
}

