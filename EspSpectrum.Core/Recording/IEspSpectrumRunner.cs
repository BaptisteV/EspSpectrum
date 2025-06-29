
namespace EspSpectrum.Core.Recording;

/// <summary>
/// Main runner of the ESP spectrum application.
/// Call WaitForNextTick on a tight loop.
/// </summary>
public interface IEspSpectrumRunner
{
    /// <summary>
    /// Starts the recording.
    /// </summary>
    void Start();

    /// <summary>
    /// Runs the main loop of the application, processing audio data and sending it to the ESP device.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    bool WaitForNextTick(CancellationToken cancellationToken);

    ValueTask DoFftAndSend(CancellationToken cancellationToken);
}