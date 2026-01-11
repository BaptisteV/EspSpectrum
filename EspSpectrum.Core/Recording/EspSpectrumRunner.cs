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
    private readonly ExecutionMonitor _execMonitor;
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
        _execMonitor = new(_logger);
    }

    public async Task StartAudio(CancellationToken cancellationToken)
    {
        StartAudioCapture(cancellationToken);
        /*
        Subscribe(new SpectrumObserver(_logger, async spectrum =>
        {
            _timingMonitor.NotifyFFTSent(DateTimeOffset.UtcNow);
        }));
        */
        //await _timingMonitor.LogSummaryLoop();
        _execMonitor.StartLogLoop();
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

    private readonly Stopwatch _sw2 = new();
    private readonly Stopwatch _swSleep = new();
    public async Task<RunnerState> Loop(CancellationToken cancellationToken)
    {
        if (!_state.HasFlag(RunnerState.LoopAudioCapture))
            return _state;

        if (!_sw2.IsRunning)
            _sw2.Start();

        var spectrum = _spectrumReader.ReadBlockingSync(cancellationToken);
        await _ws.SendSpectrum(spectrum, cancellationToken);
        Parallel.ForEach(_observers, o =>
        {
            o.OnNext(spectrum);
        });
        var elapsed = _sw2.Elapsed;
        if (elapsed > _sendInterval)
        {
            _logger.LogTrace("Processing took longer ({Elapsed:n2}ms) than the send interval ({SendInterval:n2}ms)", elapsed.TotalMilliseconds, _sendInterval.TotalMilliseconds);
        }
        else
        {
            _logger.LogTrace("Processing took {Elapsed}ms, sleeping for {Sleep}ms", elapsed.TotalMilliseconds, (_sendInterval - elapsed).TotalMilliseconds);
            _swSleep.Restart();
            var sleepFor = _sendInterval - elapsed;
            await _sleep.Wait(sleepFor, cancellationToken);
            var actualSleep = _swSleep.Elapsed;
            var diff = actualSleep - sleepFor;
            var diffSign = diff > TimeSpan.Zero ? "Sleep too long  by"
                                                : "Sleep too short by";
            if (diff.Duration() >= TimeSpan.FromMilliseconds(1))
            {
                _logger.LogDebug("{DiffSign} {SleepDiff:n2}ms. Slept for: {SleepDuration:n2}ms, expected {ExpectedSleep:n2}ms",
                    diffSign, diff.TotalMilliseconds, actualSleep.TotalMilliseconds, sleepFor.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug("{DiffSign} {SleepDiff:n2}ms. Slept for: {SleepDuration:n2}ms, expected {ExpectedSleep:n2}ms",
                    diffSign, diff.TotalMilliseconds, actualSleep.TotalMilliseconds, sleepFor.TotalMilliseconds);
            }
        }

        var executedIn = _sw2.Elapsed.TotalMilliseconds;

        _sw2.Restart();

        _execMonitor.NotifytLoopDone(executedIn);
        _logger.LogTrace("LoopDone after {FFTTime:n2}ms", executedIn);

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

