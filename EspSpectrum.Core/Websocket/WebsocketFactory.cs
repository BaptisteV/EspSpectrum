using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public class WebsocketFactory(IOptions<EspConfig> config) : IWebsocketFactory
{
    private readonly IOptions<EspConfig> _config = config;

    public WebsocketClient CreateClient(ILogger logger)
    {
        var client = new WebsocketClient(new Uri(_config.Value.EspIp))
        {
            ErrorReconnectTimeout = TimeSpan.FromMilliseconds(500),
            ReconnectTimeout = null,
        };

        client.DisconnectionHappened.Subscribe((disconnectInfo) =>
        {
            if (disconnectInfo.CloseStatus != System.Net.WebSockets.WebSocketCloseStatus.NormalClosure)
            {
                logger.LogWarning("Esp websocket disconnected. Reason: {DisconnectType}", disconnectInfo.Type);
            }
        });

        client.ReconnectionHappened.Subscribe(reconnectionInfo =>
            {
                if (reconnectionInfo.Type == ReconnectionType.Initial)
                {
                    logger.LogInformation("Esp websocket connected successfully");
                }
                else
                {
                    logger.LogInformation("Esp websocket reconnected. Reason: {ReconnectType}", reconnectionInfo.Type);
                }
            });

        return client;
    }
}
