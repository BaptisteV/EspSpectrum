namespace EspSpectrum.Core.Recording;

public interface IAudioRecorder
{
    int SampleRate { get; }
    Task<float[]> ReadN(int length);
    void Restart();
}