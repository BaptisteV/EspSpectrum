using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

public interface IFftRecorder
{
    Spectrum? TryReadFft(CancellationToken cancellationToken = default);
    void Start();
    void Restart();
}