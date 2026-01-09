using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Websocket;

/// <summary>
/// Sends the spectrum data to the ESP device.
/// </summary>
public interface ISpectrumWebsocket : IEspWebsocket
{
    /// <summary>
    /// Sends the spectrum data to the ESP device.
    /// </summary>
    /// <param name="spectrum"></param>
    /// <returns></returns>
    ValueTask SendSpectrum(Spectrum spectrum, CancellationToken cancellationToken);
}