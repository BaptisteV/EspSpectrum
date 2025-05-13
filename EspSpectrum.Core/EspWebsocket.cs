using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using Websocket.Client;

namespace EspSpectrum.Core;

public sealed class EspWebsocket : IWebsocketBars
{
    private readonly WebsocketClient _wsClient;
    private readonly ILogger<EspWebsocket> _logger;
    private bool _starting = false;

    public EspWebsocket(IOptions<EspConfig> config, ILogger<EspWebsocket> logger)
    {
        _logger = logger;
        _wsClient = EspConfig.GetWebsocketClient(config.Value.EspAdress, _logger);
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
            _logger.LogError(ce, "Operation cancelled, ESP restarting ?");
        }
    }
}