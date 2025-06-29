using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Core.Fft;

public sealed class SyncSpectrumReader(IFftRecorder recorder, IOptions<SpectrumConfig> spectrumConfig) : ISyncSpectrumReader, IDisposable
{
    private readonly TimeSpan TryInterval = TimeSpan.FromMicroseconds(500);
    private readonly IFftRecorder _recorder = recorder;
    private readonly SpectrumConfig _spectrumConfig = spectrumConfig.Value;

    public Spectrum GetLatestBlocking(CancellationToken cancellationToken)
    {
        Spectrum? nullableSpectrum;
        while (!_recorder.TryReadSpectrum(out nullableSpectrum, cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            PreciseSleep.Wait(TryInterval, cancellationToken);
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
