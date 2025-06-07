using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
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

    private async Task WaitIfNecessary(TimeSpan swElapsed, TimeSpan target)
    {
        if (swElapsed > target)
        {
            _logger.LogInformation("Getting too slow ({Elapsed}ms elapsed, Target is {TargetRate}). " +
                "Consider increasing TargetRate or decreasing ReadLength ({ReadLength})",
                swElapsed.TotalMilliseconds, target.TotalMilliseconds, FftProps.ReadLength);
            return;
        }

        var remainingTime = target - swElapsed;
        await Task.Delay(remainingTime);
    }

    public async IAsyncEnumerable<Spectrum> NextFft([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested)
        {
            /*
            stopwatch.Restart();
            
            var fft = await _audioRecorder.ReadFft(cancellationToken);

            if (_spectrumConfig.ApplyCompression)
            {
                fft.Bands = SpectrumCompressor.Compress(fft.Bands, _spectrumConfig.Compression.Threshold, _spectrumConfig.Compression.Ratio);
            }

            await WaitIfNecessary(stopwatch.Elapsed, _configMonitor.CurrentValue.SendInterval);
            yield return fft;*/

            sw.Restart();
            var s = _audioRecorder.TryReadFft();
            if (s is null)
            {
                //_logger.LogDebug("No FFT available, waiting for next one");
            }
            else
            {
                if (_spectrumConfig.ApplyCompression)
                {
                    s.Bands = SpectrumCompressor.Compress(s.Bands, _spectrumConfig.Compression.Threshold, _spectrumConfig.Compression.Ratio);
                }
                yield return s;
            }

            await WaitIfNecessary(sw.Elapsed, _configMonitor.CurrentValue.SendInterval);
        }
    }

    public void Start()
    {
        _audioRecorder.Start();
    }
}
