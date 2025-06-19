using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using MethodTimer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace EspSpectrum.Core.Fft;

public sealed class SpectrumStream(
    IFftRecorder audioRecorder,
    IOptionsMonitor<DisplayConfig> displayConfigMonitor,
    IOptions<SpectrumConfig> spectrumConfigMonitor,
    ILogger<SpectrumStream> logger) : ISpectrumStream
{
    private readonly IFftRecorder _audioRecorder = audioRecorder;
    private readonly IOptionsMonitor<DisplayConfig> _configMonitor = displayConfigMonitor;
    private readonly SpectrumConfig _spectrumConfig = spectrumConfigMonitor.Value;
    private readonly ILogger<SpectrumStream> _logger = logger;

    private void WaitIfNecessary(TimeSpan swElapsed, TimeSpan target)
    {
        if (swElapsed > target)
        {
            _logger.LogInformation("Getting too slow ({Elapsed}ms elapsed, Target is {TargetRate}). " +
                "Consider increasing TargetRate or decreasing ReadLength ({ReadLength})",
                swElapsed.TotalMilliseconds, target.TotalMilliseconds, FftProps.ReadLength);
            return;
        }

        var remainingTime = target - swElapsed;

        PreciseSleep.Wait(remainingTime);
    }

    public async IAsyncEnumerable<Spectrum> NextFft([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested)
        {
            sw.Restart();
            yield return ProcessAudio(cancellationToken);
            WaitIfNecessary(sw.Elapsed, _configMonitor.CurrentValue.SendInterval);
        }
    }

    [Time]
    private Spectrum ProcessAudio(CancellationToken cancellationToken)
    {
        Spectrum? s = null;
        while (!_audioRecorder.TryReadSpectrum(out s, cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            PreciseSleep.Wait(TimeSpan.FromMilliseconds(1));
        }

        if (_spectrumConfig.ApplyCompression)
        {
            s.Bands = SpectrumCompressor.Compress(s.Bands, _spectrumConfig.Compression.Threshold, _spectrumConfig.Compression.Ratio);
        }
        return s;
    }

    public void Start()
    {
        _audioRecorder.Start();
    }
}
