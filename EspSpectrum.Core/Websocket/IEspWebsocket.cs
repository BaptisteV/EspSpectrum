namespace EspSpectrum.Core.Websocket;

public interface IEspWebsocket
{
    Task<bool> TryConnect(CancellationToken cancellationToken);
    Task ReconnectLoop(CancellationToken cancellationToken);
    bool IsConnected();
}
