using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

public interface IFftRecorder
{
    int SampleRate { get; }
    Task<Spectrum> ReadFft(CancellationToken cancellationToken = default);
    void Restart();
}