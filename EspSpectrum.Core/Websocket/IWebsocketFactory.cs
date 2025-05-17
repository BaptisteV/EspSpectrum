using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public interface IWebsocketFactory
{
    WebsocketClient CreateClient(ILogger logger);
}
