using Microsoft.Extensions.Logging;

namespace EspSpectrum.Core.Fft;

public class SpectrumObserver(ILogger logger, Func<Spectrum, Task> updateUi)
{
    private readonly ILogger _logger = logger;
    private readonly Func<Spectrum, Task> updateUi = updateUi;

    public async Task OnNext(Spectrum value)
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