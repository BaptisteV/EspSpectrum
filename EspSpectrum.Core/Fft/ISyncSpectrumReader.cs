namespace EspSpectrum.Core.Fft;

public interface ISyncSpectrumReader
{
    public void Start();
    Spectrum GetLatestBlocking(CancellationToken cancellationToken);
}