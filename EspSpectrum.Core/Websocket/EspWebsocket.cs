using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text.Json;
using Websocket.Client;

namespace EspSpectrum.Core.Websocket;

public sealed class EspWebsocket : ISpectrumWebsocket, IDisplayConfigWebsocket, IDisposable
{
    private readonly WebsocketClient _ws;
    private readonly IOptions<EspConfig> _config;
    private readonly ILogger<EspWebsocket> _logger;

    public EspWebsocket(IOptions<EspConfig> config, ILogger<EspWebsocket> logger)
    {
        this._config = config;
        _logger = logger;
        _ws = CreateClient();
    }

    public WebsocketClient CreateClient()
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
                _logger.LogInformation("Esp websocket disconnected normally.");
            }
            else
            {
                _logger.LogWarning("Esp websocket disconnected. Reason: {DisconnectType} {Message}", disconnectInfo.Type.ToString(), disconnectInfo.Exception?.ToString());
            }
        });

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

    public async ValueTask SendDisplayConfig(DisplayConfig displayConfig, CancellationToken cancellationToken)
    {
        var jsonString = JsonSerializer.Serialize(displayConfig);
        await _ws.SendInstant(jsonString);
    }

    public async ValueTask SendSpectrum(Spectrum spectrum, CancellationToken cancellationToken)
    {
        var packedData = PackData([.. spectrum.Bands.Select(b => (int)Math.Round(b))]);
        try
        {
            await _ws.SendInstant(packedData);
        }
        catch (SocketException se)
        {
            _logger.LogError(se, "Connection error");
        }
        catch (OperationCanceledException ce)
        {
            _logger.LogError(ce, "Operation cancelled, ESP restarting ?");
        }
    }

    public void Dispose()
    {
        _ws.Dispose();
    }

    public Task ReconnectLoop(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var periodicTimer = new PeriodicTimer(_ws.ConnectTimeout + TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (!IsConnected())
                {
                    await TryConnect(cancellationToken);
                }
            }
        }).ConfigureAwait(false);
        return Task.CompletedTask;
    }

    public async Task<bool> TryConnect(CancellationToken cancellationToken)
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