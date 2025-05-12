namespace EspSpectrum.Core;

public interface IFftReader
{
    ValueTask<FftResult> ReadLastFft(CancellationToken cancellation = default);
    void Restart();
}