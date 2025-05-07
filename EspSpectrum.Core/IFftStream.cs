namespace EspSpectrum.Core;

public interface IFftStream
{
    IAsyncEnumerable<FftResult> NextFft(CancellationToken cancellationToken = default);
}