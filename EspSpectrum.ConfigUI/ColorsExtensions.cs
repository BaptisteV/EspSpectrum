namespace EspSpectrum.ConfigUI;

public static class ColorsExtensions
{
    private static float MapRange(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var scaled = (value - fromMin) / (fromMax - fromMin);
        return toMin + scaled * (toMax - toMin);
    }

    public static int GetInt8Hue(this Color color)
    {
        return (int)MapRange(color.GetHue(), 0, 360, 0, 255);
    }
}
