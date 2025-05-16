namespace EspSpectrum.Core.Fft;

public interface IFftStream
{
    IAsyncEnumerable<FftResult> NextFft(CancellationToken cancellationToken = default);
}