using NAudio.Dsp;

namespace EspSpectrum.Core;

public interface IFftReader
{
    int[] CalculateBands(Complex[] fftResult);
    int AvailableSamples();
    Task<FftResult> ReadLastFft();
}