
namespace EspSpectrum.Core;

public interface IAudioRecorder
{
    int RecordedSamples { get; }
    int SampleRate { get; }
    Task<float[]> ReadN(int length);
}