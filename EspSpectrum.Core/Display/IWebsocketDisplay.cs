namespace EspSpectrum.Core.Display;

public interface IWebsocketDisplay
{
    Task Send(DisplayConfig displayConfig);
}
