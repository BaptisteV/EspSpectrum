using EspSpectrum.Core.Display;

namespace EspSpectrum.Core.Websocket;

/// <summary>
/// Sends the display configuration to the ESP device.
/// </summary>
public interface IDisplayConfigWebsocket : IDisposable
{
    /// <summary>
    /// Sends the display configuration to the ESP device.
    /// </summary>
    /// <param name="displayConfig"></param>
    /// <returns></returns>
    ValueTask SendDisplayConfig(DisplayConfig displayConfig);
}
