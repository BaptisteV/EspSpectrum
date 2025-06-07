namespace EspSpectrum.Core.Recording;
using System.Collections.Generic;

public interface IDataReader
{
    void AddData(ReadOnlySpan<float> newData);
    bool TryRead(out float[] data);
    List<float[]> ReadAll();
}