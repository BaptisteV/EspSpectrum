using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core;
public class FftStream(IFftReader fftReader, ILogger<FftStream> logger) : IFftStream
{
    private readonly ILogger<FftStream> _logger = logger;
    private readonly IFftReader _fftReader = fftReader;

    private async Task WaitIfNecessary(TimeSpan swElapsed, TimeSpan target)
    {
        if (swElapsed > target)
        {
            _logger.LogWarning("Getting too slow ({Elapsed}ms elapsed, Target is {TargetRate}). " +
                "Consider increasing TargetRate or decreasing ReadLength ({ReadLength})",
                swElapsed.TotalMilliseconds, target.TotalMilliseconds, BandsConfig.ReadLength);
            return;
        }

        var remainingTime = target - swElapsed;

        if (remainingTime > TimeSpan.FromMicroseconds(100))
        {
            await Task.Delay(remainingTime);
        }
    }

    public async IAsyncEnumerable<FftResult> NextFft([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Restart();
            var fft = await _fftReader.ReadLastFft();

            await WaitIfNecessary(stopwatch.Elapsed, BandsConfig.TargetRate);
            yield return fft;
        }
    }
}
