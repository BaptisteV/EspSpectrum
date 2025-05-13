
namespace EspSpectrum.Core;

public interface IAudioRecorder
{
    int SampleRate { get; }
    int ChannelCount { get; }
    Task<float[]> ReadN(int length);
    void Restart();
}