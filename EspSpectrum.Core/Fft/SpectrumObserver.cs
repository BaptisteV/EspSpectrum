using Microsoft.Extensions.Logging;

namespace EspSpectrum.Core.Fft;

public class SpectrumObserver(ILogger logger, Action<Spectrum> updateUi)
{
    private readonly ILogger _logger = logger;
    private readonly Action<Spectrum> updateUi = updateUi;

    public void OnNext(Spectrum value)
    {
        updateUi(value);
    }
}

public interface ISpectrumObservable
{
    void Subscribe(SpectrumObserver observer);
}