namespace EspSpectrum.Core.Display;

public interface IDisplayConfigWebsocket
{
    Task Send(DisplayConfig displayConfig);
}
