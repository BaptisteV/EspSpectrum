using EspSpectrum.Core.Display;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.Json;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public sealed class EspWebsocket : ISpectrumWebsocket, IDisplayConfigWebsocket
{
    private readonly WebsocketClient _wsSpectrum;
    private readonly WebsocketClient _wsDisplayConfig;
    private readonly ILogger<EspWebsocket> _logger;
    private bool _starting = false;

    public EspWebsocket(IWebsocketFactory wsFactory, ILogger<EspWebsocket> logger)
    {
        _logger = logger;
        _wsSpectrum = wsFactory.CreateClient(_logger);
        _wsDisplayConfig = wsFactory.CreateClient(_logger);
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

    private async ValueTask ConnectIfNeeded()
    {
        if (!_wsSpectrum.IsRunning && !_starting)
        {
            _starting = true;
            _logger.LogInformation("Connecting...");
            await _wsSpectrum.Start();
            _logger.LogInformation("Connected");
            _starting = false;
        }
    }

    public async ValueTask SendDisplayConfig(DisplayConfig displayConfig)
    {
        await _wsDisplayConfig.Start();
        var jsonString = JsonSerializer.Serialize(displayConfig);
        await _wsDisplayConfig.SendInstant(jsonString);
        await _wsDisplayConfig.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed after sending display config");
    }

    public async ValueTask SendSpectrum(double[] bands)
    {
        await ConnectIfNeeded();

        var packedData = PackData([.. bands.Select(b => (int)Math.Round(b))]);
        try
        {
            await _wsSpectrum.SendInstant(packedData);
        }
        catch (SocketException se)
        {
            _logger.LogError(se, "Connection error");
            throw;
        }
        catch (OperationCanceledException ce)
        {
            _logger.LogError(ce, "Operation cancelled, ESP restarting ?");
            throw;
        }
    }
}