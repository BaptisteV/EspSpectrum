namespace EspSpectrum.Core.Fft;

public interface IFftReader
{
    ValueTask<FftResult> ReadLastFft(CancellationToken cancellation = default);
    void Restart();
}