using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

public interface IFftRecorder
{
    int SampleRate { get; }
    ValueTask<Spectrum> ReadFft(CancellationToken cancellationToken = default);
    void Start();
    void Restart();
}