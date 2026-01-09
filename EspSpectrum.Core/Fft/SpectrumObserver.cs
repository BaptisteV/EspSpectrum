using Microsoft.Extensions.Logging;

namespace EspSpectrum.Core.Fft;

public class SpectrumObserver(ILogger logger, Func<Spectrum, ValueTask> updateUi)
{
    private readonly ILogger _logger = logger;
    private readonly Func<Spectrum, ValueTask> updateUi = updateUi;

    public async ValueTask OnNext(Spectrum value)
    {
        await updateUi(value);
    }
}

public interface ISpectrumObservable
{
    void Subscribe(SpectrumObserver observer);
}

public class SpectrumObservable(ISyncSpectrumReader spectrumReader) : ISpectrumObservable
{
    private readonly ISyncSpectrumReader _spectrumReader = spectrumReader;
    public void Subscribe(SpectrumObserver observer)
    {
        _spectrumReader.Subscribe(observer);
    }
}