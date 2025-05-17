using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace EspSpectrum.Core.Fft;

public sealed class FftStream(
    IFftRecorder audioRecorder,
    IOptionsMonitor<DisplayConfig> configMonitor,
    ILogger<FftStream> logger) : IFftStream
{
    private readonly IFftRecorder _audioRecorder = audioRecorder;
    private readonly IOptionsMonitor<DisplayConfig> _configMonitor = configMonitor;
    private readonly ILogger<FftStream> _logger = logger;

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

    public async IAsyncEnumerable<FftResult> NextFft([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Restart();
            var fft = await _audioRecorder.ReadFft(cancellationToken);
            await WaitIfNecessary(stopwatch.Elapsed, _configMonitor.CurrentValue.SendInterval);
            yield return fft;
        }
    }
}
