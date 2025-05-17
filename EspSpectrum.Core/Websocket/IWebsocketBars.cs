namespace EspSpectrum.Core.Websocket;

public interface IWebsocketBars
{
    Task SendAudio(int[] audio);
}