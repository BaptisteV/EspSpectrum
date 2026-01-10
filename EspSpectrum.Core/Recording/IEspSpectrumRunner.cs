using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Recording;

/// <summary>
/// Main runner of the ESP spectrum application.
/// Call WaitForNextTick on a tight loop.
/// </summary>
public interface IEspSpectrumRunner : ISpectrumObservable
{
    /// <summary>
    /// Starts the recording.
    /// </summary>
    Task StartAudio(CancellationToken cancellationToken);

    Task<bool> TryConnectEsp(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the recording.
    /// </summary>
    /// <returns></returns>
    Task Stop();

    /// <summary>
    /// Runs the main loop of the application, processing audio data and sending it to the ESP device.
    /// </summary>
    Task<RunnerState> Loop(CancellationToken cancellationToken);
}