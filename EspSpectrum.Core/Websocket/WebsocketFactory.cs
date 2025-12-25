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
            //ConnectTimeout = TimeSpan.FromMilliseconds(1000),
            ErrorReconnectTimeout = TimeSpan.FromMilliseconds(1000),
            IsReconnectionEnabled = false,
            ReconnectTimeout = null,
        };

        client.DisconnectionHappened.Subscribe((disconnectInfo) =>
        {
            if (disconnectInfo.CloseStatus == System.Net.WebSockets.WebSocketCloseStatus.NormalClosure)
            {
                logger.LogInformation("Esp websocket disconnected normally.");
            }
            else
            {
                logger.LogWarning("Esp websocket disconnected. Reason: {DisconnectType} {Message}", disconnectInfo.Type.ToString(), disconnectInfo.Exception?.ToString());
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
