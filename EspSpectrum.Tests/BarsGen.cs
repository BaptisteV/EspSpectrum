using EspSpectrum.Core.Fft;

namespace EspSpectrum.DisplayTests;

public static class BarsGen
{
    public static double[] GetLine(int value)
    {
        var data = new double[FftProps.NBands];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = value;
        }
        return data;
    }
}
