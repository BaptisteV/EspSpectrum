namespace EspSpectrum.Core.Recording;

public interface IFftRecorder
{
    int SampleRate { get; }
    Task<FftResult> ReadFft(CancellationToken cancellationToken);
    void Restart();
}