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

    public async ValueTask SendDisplayConfig(DisplayConfig displayConfig)
    {
        var jsonString = JsonSerializer.Serialize(displayConfig);
        await _ws.SendInstant(jsonString);
    }

    public async ValueTask SendSpectrum(Spectrum spectrum)
    {
        var packedData = PackData([.. spectrum.Bands.Select(b => (int)Math.Round(b))]);
        try
        {
            await _ws.SendInstant(packedData);
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

    public void Dispose()
    {
        _ws.Dispose();
    }

    public Task TryConnectLoop()
    {
        _ = Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(_ws.ConnectTimeout + TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (!IsConnected())
                {
                    await Connect();
                }
            }
        });
        return Task.CompletedTask;
    }

    public async Task<bool> Connect()
    {
        if (IsConnected())
            return true;

        await DoConnect();

        return IsConnected();
    }

    private async Task<bool> DoConnect()
    {
        _logger.LogInformation("Connecting...");
        await _ws.Start();
        var connected = IsConnected();
        _logger.LogInformation(connected ? "Connected" : "Failed to connect");
        return connected;
    }

    public bool IsConnected() => _ws.IsRunning;
}