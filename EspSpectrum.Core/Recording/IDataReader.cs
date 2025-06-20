namespace EspSpectrum.Core.Recording;

public interface IDataReader
{
    int Count();
    void AddData(ReadOnlySpan<float> newData);
    bool TryReadAudioFrame(Span<float> data);
}