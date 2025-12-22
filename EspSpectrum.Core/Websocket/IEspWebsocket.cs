namespace EspSpectrum.Core.Websocket;

public interface IEspWebsocket
{
    Task<bool> TryConnectInBg();
    bool IsConnected();
}
