namespace EspSpectrum.Core.Recording;
public interface IDataReader
{
    void AddData(ReadOnlySpan<float> newData);
    bool TryRead(out float[] data);
}