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
    private readonly TimeSpan _sendInterval;
    private readonly Stopwatch _stopwatch = new();
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

    public async ValueTask<Spectrum> DoFftAndSend(CancellationToken cancellationToken)
    {
        var spectrum = await _spectrumReader.GetLatestBlocking(cancellationToken);
        await _ws.SendSpectrum(spectrum);
        return spectrum;
    }

    public async Task Start()
    {
        _spectrumReader.Start();

        await _timingMonitor.StartInBg();
        await _ws.TryConnectInBg();
        _stopwatch.Start();
    }

    public async Task<Spectrum> Loop(CancellationToken cancellationToken)
    {
        //var gcLocked = GC.TryStartNoGCRegion(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 10);
        //if (!gcLocked)
        //{
        //    _logger.LogWarning("Could not enter No GC Region");
        //}

        var spectrum = await ProcessFftAndSend(cancellationToken);
        //GC.EndNoGCRegion();

        return spectrum;
    }

    private async Task<Spectrum> ProcessFftAndSend(CancellationToken cancellationToken)
    {
        _stopwatch.Restart();
        var spectrum = await DoFftAndSend(cancellationToken);
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
        return spectrum;
    }
}

