namespace EspSpectrum.Core.Fft;

public static class SpectrumCompressor
{
    /// <summary>
    /// Compresses dynamic range of double array with optimized performance
    /// </summary>
    /// <param name="data">Input array (0.0 to 8.0 range expected)</param>
    /// <param name="threshold">Compression starts above this value (default: 6.0)</param>
    /// <param name="ratio">Compression ratio for values above threshold (default: 3.0)</param>
    /// <returns>New compressed array</returns>
    public static double[] Compress(double[] data, double threshold = 6.0, double ratio = 3.0)
    {
        var result = new double[data.Length];
        var invRatio = 1.0 / ratio; // Pre-calculate to avoid division in loop

        for (int i = 0; i < data.Length; i++)
        {
            var value = data[i];

            // Fast path: no compression needed
            if (value <= threshold)
            {
                result[i] = value;
            }
            else
            {
                // Compress excess above threshold
                var excess = value - threshold;
                result[i] = threshold + excess * invRatio;
            }
        }

        return result;
    }
}
