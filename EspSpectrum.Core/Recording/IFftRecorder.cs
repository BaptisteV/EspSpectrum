using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

public interface IFftRecorder
{
    bool TryReadSpectrum(out Spectrum? spectrum, CancellationToken cancellationToken);
    void Start();
    void Restart();
}