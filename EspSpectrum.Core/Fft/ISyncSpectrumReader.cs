namespace EspSpectrum.Core.Fft;

/// <summary>
/// Read spectrum data synchronously.
/// </summary>
public interface ISyncSpectrumReader
{
    /// <summary>
    /// Starts the recording.
    /// </summary>
    void Start();

    /// <summary>
    /// Gets the latest spectrum data. This method blocks until data is available.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Spectrum> ReadBlockingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the latest spectrum data. This method blocks until data is available.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Spectrum ReadBlockingSync(CancellationToken cancellationToken);
}