namespace EspSpectrum.Core.Fft;

public class SpectrumConfig
{
    public class CompressionConfig
    {
        public double Threshold { get; set; } = 4;
        public double Ratio { get; set; } = 3;
    }

    public bool ApplyCompression { get; set; } = true;
    public CompressionConfig Compression { get; set; } = new();
}
