namespace EspSpectrum.Core.Websocket;

public interface IEspWebsocket
{
    Task<bool> TryConnect();
    Task TryConnectLoop();
    bool IsConnected();
}
