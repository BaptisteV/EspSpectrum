using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using Websocket.Client;

namespace EspSpectrum.Core;

public sealed class EspWebsocket : IEspWebsocket
{
    private readonly WebsocketClient _wsClient;
    private readonly ILogger<EspWebsocket> _logger;
    private bool _starting = false;

    public EspWebsocket(EspSpectrumConfig config, ILogger<EspWebsocket> logger)
    {
        _wsClient = GetWebsocketClient(config.EspAdress);
        _logger = logger;
    }

    private WebsocketClient GetWebsocketClient(Uri espAdress)
    {
        var client = new WebsocketClient(espAdress)
        {
            ErrorReconnectTimeout = TimeSpan.FromMilliseconds(500),
            ReconnectTimeout = null
        };

        client.DisconnectionHappened.Subscribe(disconnectInfo
            => _logger.LogWarning("Esp websocket disconnected. Reason: {DisconnectType}", disconnectInfo.Type));

        client.ReconnectionHappened.Subscribe(reconnectionInfo =>
        {
            if (reconnectionInfo.Type == ReconnectionType.Initial)
            {
                _logger.LogInformation("Esp websocket connected successfully");
            }
            else
            {
                _logger.LogInformation("Esp websocket reconnected. Reason: {ReconnectType}", reconnectionInfo.Type);
            }
        });

        return client;
    }

    private static byte[] PackData(int[] bars)
    {
        // Create a new byte array to hold the packed data
        var packedData = new byte[bars.Length / 2];

        // Pack two 4-bit values into one byte
        for (var i = 0; i < bars.Length; i += 2)
        {
            var firstValue = (byte)(bars[i] & 0x0F); // Mask to get the lower 4 bits
            var secondValue = (byte)(bars[i + 1] & 0x0F); // Mask to get the lower 4 bits
            packedData[i / 2] = (byte)(firstValue << 4 | secondValue); // Combine into one byte
        }

        return packedData;
    }

    public async Task SendAudio(int[] audio)
    {
        if (!_wsClient.IsRunning && !_starting)
        {
            _starting = true;
            _logger.LogInformation("Connecting...");
            await _wsClient.Start();
            _logger.LogInformation("Connected");
            _starting = false;
        }

        var packedData = PackData(audio);
        try
        {
            await _wsClient.SendInstant(packedData);
        }
        catch (SocketException se)
        {
            // SocketException (10054): An existing connection was forcibly closed by the remote host
            /*if (se.ErrorCode != (int)SocketError.ConnectionReset)
            {
                throw;
            }*/
            _logger.LogError(se, "Connection error");
            throw;
        }
        catch (OperationCanceledException ce)
        {
            _logger.LogError(ce, "Operation cancelled");
            throw;
        }

    }
}