namespace EspSpectrum.Core.Display;

public sealed class DisplayConfig : IEquatable<DisplayConfig>
{
    public TimeSpan SendInterval { get; set; }

    public double Amplification { get; set; } = 1.0;

    // Sent to ESP when appsettings is updated
    public int HistoLength { get; set; }
    public int Brightness { get; set; }
    public int LowHue { get; set; }
    public int MidHue { get; set; }
    public int HighHue { get; set; }

    public bool Equals(DisplayConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return SendInterval.Equals(other.SendInterval) &&
               Amplification.Equals(other.Amplification) &&
               HistoLength == other.HistoLength &&
               Brightness == other.Brightness &&
               LowHue == other.LowHue &&
               MidHue == other.MidHue &&
               HighHue == other.HighHue;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals(obj as DisplayConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            SendInterval,
            Amplification,
            HistoLength,
            Brightness,
            LowHue,
            MidHue,
            HighHue);
    }

    public static bool operator ==(DisplayConfig left, DisplayConfig right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(DisplayConfig left, DisplayConfig right)
    {
        return !(left == right);
    }
}
