using EspSpectrum.Core.Display;

namespace EspSpectrum.Core.Websocket;

public interface IDisplayConfigWebsocket
{
    Task SendDisplayConfig(DisplayConfig displayConfig);
}
