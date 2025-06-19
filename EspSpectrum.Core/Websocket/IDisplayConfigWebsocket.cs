using EspSpectrum.Core.Display;

namespace EspSpectrum.Core.Websocket;

public interface IDisplayConfigWebsocket
{
    ValueTask SendDisplayConfig(DisplayConfig displayConfig);
}
