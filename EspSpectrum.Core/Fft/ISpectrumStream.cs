namespace EspSpectrum.Core.Fft;

public interface ISpectrumStream
{
    void Start();
    IAsyncEnumerable<Spectrum> NextFft(CancellationToken cancellationToken = default);
}