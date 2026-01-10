using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Core.Fft;

public sealed class SyncSpectrumReader(
    IFftRecorder recorder,
    IPreciseSleep sleep,
    IOptions<SpectrumConfig> spectrumConfig,
    IOptionsMonitor<DisplayConfig> displayConfigMonitor) : ISyncSpectrumReader, IDisposable
{
    private readonly TimeSpan TryInterval = displayConfigMonitor.CurrentValue.SendInterval / 2;
    private readonly IFftRecorder _recorder = recorder;
    private readonly SpectrumConfig _spectrumConfig = spectrumConfig.Value;
    private readonly IPreciseSleep _sleep = sleep;

    public async Task<Spectrum> ReadBlocking(CancellationToken cancellationToken)
    {
        Spectrum? nullableSpectrum;
        while (!_recorder.TryReadSpectrum(out nullableSpectrum, cancellationToken))
        {
            //await _sleep.Wait(TryInterval, cancellationToken);
            Thread.Sleep(1);
        }

        Spectrum foundSpectrum = nullableSpectrum ?? throw new InvalidOperationException($"{nameof(nullableSpectrum)} should never be null here");
        if (_spectrumConfig.ApplyCompression)
        {
            foundSpectrum.Bands = SpectrumCompressor.Compress(foundSpectrum.Bands, _spectrumConfig.Compression.Threshold, _spectrumConfig.Compression.Ratio);
        }

        return foundSpectrum;
    }

    public void Start()
    {
        _recorder.Start();
    }

    public void Dispose()
    {
        _recorder.Dispose();
    }
}
