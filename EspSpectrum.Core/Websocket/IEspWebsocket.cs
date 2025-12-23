namespace EspSpectrum.Core.Websocket;

public interface IEspWebsocket
{
    Task<bool> Connect();
    Task TryConnectLoop();
    bool IsConnected();
}
