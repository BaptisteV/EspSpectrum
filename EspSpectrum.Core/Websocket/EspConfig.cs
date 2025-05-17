using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public class EspConfig
{
    public string EspIp { get; set; } = "";

    public WebsocketClient GetWebsocketClient(ILogger logger)
    {
        var client = new WebsocketClient(new Uri(EspIp))
        {
            ErrorReconnectTimeout = TimeSpan.FromMilliseconds(500),
            ReconnectTimeout = null
        };

        client.DisconnectionHappened.Subscribe(disconnectInfo
            => logger.LogWarning("Esp websocket disconnected. Reason: {DisconnectType}", disconnectInfo.Type));

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
