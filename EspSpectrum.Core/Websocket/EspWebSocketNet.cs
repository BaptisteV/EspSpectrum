using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace EspSpectrum.Core.Websocket;

public sealed class EspWebsocketNet : ISpectrumWebsocket, IDisplayConfigWebsocket
{
    private readonly ClientWebSocket _ws;
    private readonly ILogger<EspWebsocketNet> _logger;
    private readonly Uri _uri;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(5));
    public EspWebsocketNet(IOptions<EspConfig> cfg, ILogger<EspWebsocketNet> logger)
    {
        _logger = logger;
        _uri = new Uri(cfg.Value.EspIp);
        _ws = new ClientWebSocket()
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.Zero,
            },
        };
    }

    private static byte[] PackData(int[] bars)
    {
        var data = new byte[bars.Length / 2];
        for (var i = 0; i < bars.Length; i += 2)
        {
            var a = (byte)(bars[i] & 0xF);
            var b = (byte)(bars[i + 1] & 0xF);
            data[i / 2] = (byte)((a << 4) | b);
        }
        return data;
    }

    public async ValueTask SendDisplayConfig(DisplayConfig config, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(config);
        var buffer = Encoding.UTF8.GetBytes(json);
        await SendAsync(buffer, WebSocketMessageType.Text, cancellationToken);
    }

    public async ValueTask SendSpectrum(Spectrum spectrum, CancellationToken cancellationToken)
    {
        var packed = PackData([.. spectrum.Bands.Select(x => (int)Math.Round(x))]);
        await SendAsync(packed, WebSocketMessageType.Binary, cancellationToken);
    }

    private async Task SendAsync(byte[] payload, WebSocketMessageType type, CancellationToken cancellationToken)
    {
        if (!IsConnected())
        {
            _logger.LogDebug("WebSocket not connected, cannot send data");
            return;
        }

        try
        {
            await _ws.SendAsync(payload, type, true, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Send canceled: ESP reset?");
        }
        catch (WebSocketException wsException)
        {
            _logger.LogError(wsException, "WebSocket error when sending spectrum");
        }
    }

    public bool IsConnected() =>
        _ws.State == WebSocketState.Open;

    public async Task<bool> TryConnect(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting...");
        try
        {
            if (IsConnected())
            {
                _logger.LogInformation("Already connected");
                return true;
            }

            if (_ws.State != WebSocketState.Connecting)
                await _ws.ConnectAsync(_uri, cancellationToken);

            if (_ws.State == WebSocketState.Open)
            {
                _logger.LogInformation("Connected");
                return true;
            }

            _logger.LogError("Failed to connect");
            return false;
        }
        catch (WebSocketException wsException)
        {
            _logger.LogError(wsException, "Error connecting to ESP WebSocket at start");
            return false;
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogError(canceledException, "Timeout connecting to ESP WebSocket at start");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error connecting to ESP WebSocket at start");
            return false;
        }
    }

    public Task ReconnectLoop(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    if (!IsConnected())
                        await TryConnect(cancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "TryConnect in ReconnectLoop canceled");
                    // normal shutdown
                }
                catch (WebSocketException wsException)
                {
                    _logger.LogDebug(wsException, "WebSocket exception in ReconnectLoop");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocketNet connect loop error");
                    throw;
                }
            }

        }, cancellationToken).ConfigureAwait(false);

        return Task.CompletedTask;
    }
    /*
    public async ValueTask DisposeAsync()
    {
        await _ws.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure, "DisposeAsync close", CancellationToken.None);

        _ws.Dispose();
        _timer.Dispose();
    }*/
}
