using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Core.Fft;

public sealed class SyncSpectrumReader(IFftRecorder recorder, IPreciseSleep sleep, IOptions<SpectrumConfig> spectrumConfig) : ISyncSpectrumReader, IDisposable
{
    private readonly TimeSpan TryInterval = TimeSpan.FromMilliseconds(0.5);
    private readonly IFftRecorder _recorder = recorder;
    private readonly SpectrumConfig _spectrumConfig = spectrumConfig.Value;
    private readonly IPreciseSleep _sleep = sleep;

    public async Task<Spectrum> GetLatestBlocking(CancellationToken cancellationToken)
    {
        Spectrum? nullableSpectrum;
        while (!_recorder.TryReadSpectrum(out nullableSpectrum, cancellationToken))
        {
            await _sleep.Wait(TryInterval, cancellationToken);
        }

        Spectrum foundSpectrum = nullableSpectrum ?? throw new InvalidOperationException($"{nameof(nullableSpectrum)} should never be null here");
        if (_spectrumConfig.ApplyCompression)
        {
            foundSpectrum.Bands = SpectrumCompressor.Compress(foundSpectrum.Bands, _spectrumConfig.Compression.Threshold, _spectrumConfig.Compression.Ratio);
        }

        foreach (var observer in _observers)
        {
            await observer.OnNext(foundSpectrum);
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

    private readonly HashSet<SpectrumObserver> _observers = new();

    public void Subscribe(SpectrumObserver observer)
    {
        // Check whether observer is already registered. If not, add it.
        if (_observers.Add(observer))
        {
            // Provide observer with existing data.
            //foreach (Spectrum item in _flights)
            //{
            // observer.OnNext(item);
            //}
        }
    }

}
