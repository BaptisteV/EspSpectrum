using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace EspSpectrum.Core;

public class EspConfig
{
    public Uri EspAdress { get; set; } = new Uri("ws://192.168.1.133:81");

    public static WebsocketClient GetWebsocketClient(Uri espAdress, ILogger logger)
    {
        var client = new WebsocketClient(espAdress)
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
