using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using static EspSpectrum.Core.Recording.IEspSpectrumRunner;

namespace EspSpectrum.Core.Recording;


public sealed class EspSpectrumRunner : IEspSpectrumRunner
{
    private RunnerState _state;
    private readonly TimeSpan _sendInterval;
    private readonly ISyncSpectrumReader _spectrumReader;
    private readonly ISpectrumWebsocket _ws;
    private readonly ITickTimingMonitor _timingMonitor;
    private readonly IPreciseSleep _sleep;
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

    private async Task StartReconnectLoop(CancellationToken cancellationToken)
    {
        await _ws.ReconnectLoop(cancellationToken);
        _state |= RunnerState.ConnectedToEsp;
    }
    /*
    private async Task ConnectStart()
    {
        try
        {
            _startCts = new();
            _startCts.CancelAfter(TimeSpan.FromSeconds(5));
            var connected = await _ws.TryConnect(_startCts.Token);
            if (connected)
            {
                _state |= RunnerState.ConnectedToEsp;
            }
            else
            {
                _logger.LogWarning("Could not connect to ESP WebSocket at start");
                _state &= ~RunnerState.ConnectedToEsp;
            }
        }
        catch (WebsocketException wsException)
        {
            _logger.LogError(wsException, "Error connecting to ESP WebSocket at start");
            _state &= ~RunnerState.ConnectedToEsp;
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogError(canceledException, "Timeout connecting to ESP WebSocket at start");
            _state &= ~RunnerState.ConnectedToEsp;
        }
        finally
        {
            _startCts?.Dispose();
        }
    }*/

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
        await _spectrumReader.GetLatestBlockingAndNotifyObservers(cancellationToken);

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
        _spectrumReader.Subscribe(observer);
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }
}

