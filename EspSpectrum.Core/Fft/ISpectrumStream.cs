namespace EspSpectrum.Core.Fft;

public interface ISpectrumStream
{
    IAsyncEnumerable<Spectrum> NextFft(CancellationToken cancellationToken = default);
}