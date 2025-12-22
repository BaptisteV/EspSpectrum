using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

/// <summary>
/// Record audio data and compute FFT.
/// </summary>
public interface IFftRecorder : IDisposable
{
    /// <summary>
    /// Tries to read audio data and compute FFT.
    /// </summary>
    /// <param name="spectrum"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    bool TryReadSpectrum(out Spectrum? spectrum, CancellationToken cancellationToken);

    /// <summary>
    /// Starts the recording process and begins reading audio data.
    /// </summary>
    void Start();

    /// <summary>
    /// Restarts the recording. Typically used when the audio input device changes.
    /// </summary>
    void Restart();
}