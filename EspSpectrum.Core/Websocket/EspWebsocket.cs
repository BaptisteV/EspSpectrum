using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text.Json;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public sealed class EspWebsocket : ISpectrumWebsocket, IDisplayConfigWebsocket
{
    private readonly WebsocketClient _ws;
    private readonly ILogger<EspWebsocket> _logger;
    private bool _connecting = false;

    public EspWebsocket(IWebsocketFactory wsFactory, ILogger<EspWebsocket> logger)
    {
        _logger = logger;
        _ws = wsFactory.CreateClient(_logger);
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

    private bool IsConnected() => _ws.IsRunning && !_connecting;

    private async ValueTask Connect()
    {
        _logger.LogInformation("Connecting...");
        _connecting = true;
        await _ws.Start();
        _logger.LogInformation("Connected");
        _connecting = false;
    }

    public async ValueTask SendDisplayConfig(DisplayConfig displayConfig)
    {
        if (!IsConnected())
            await Connect();

        var jsonString = JsonSerializer.Serialize(displayConfig);
        await _ws.SendInstant(jsonString);
    }

    public async ValueTask SendSpectrum(Spectrum spectrum)
    {
        if (!IsConnected())
            await Connect();

        var packedData = PackData([.. spectrum.Bands.Select(b => (int)Math.Round(b))]);
        try
        {
            await _ws.SendInstant(packedData);
        }
        catch (SocketException se)
        {
            _logger.LogError(se, "Connection error");
            throw new SocketException(se.ErrorCode, se.Message);
        }
        catch (OperationCanceledException ce)
        {
            _logger.LogError(ce, "Operation cancelled, ESP restarting ?");
            throw new OperationCanceledException("Operation cancelled, ESP restarting ?", ce);
        }
    }

    public void Dispose()
    {
        _ws.Dispose();
    }
}