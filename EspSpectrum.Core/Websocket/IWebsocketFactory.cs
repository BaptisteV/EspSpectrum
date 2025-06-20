using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public interface IWebsocketFactory
{
    /// <summary>
    /// Creates a WebsocketClient instance to communicate with the ESP device..
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    WebsocketClient CreateClient(ILogger logger);
}
