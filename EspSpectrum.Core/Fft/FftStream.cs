using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace EspSpectrum.Core.Fft;

public sealed class FftStream : IFftStream, IDisposable
{
    private readonly IFftRecorder _audioRecorder;
    private readonly IOptionsMonitor<DisplayConfig> _configMonitor;
    private readonly ILogger<FftStream> _logger;

    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly DeviceChangedNotifier _deviceChangedNotifier;

    public FftStream(
        IFftRecorder audioRecorder,
        IOptionsMonitor<DisplayConfig> configMonitor,
        ILogger<FftStream> logger)
    {
        _audioRecorder = audioRecorder;
        _configMonitor = configMonitor;
        _logger = logger;

        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceChangedNotifier = new DeviceChangedNotifier(_logger, _audioRecorder);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceChangedNotifier);
    }

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

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}
