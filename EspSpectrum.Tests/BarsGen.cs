namespace EspSpectrum.DisplayTests;

public static class BarsGen
{
    public static int[] GetLine(int value)
    {
        var data = new int[32];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = value;
        }
        return data;
    }
}
