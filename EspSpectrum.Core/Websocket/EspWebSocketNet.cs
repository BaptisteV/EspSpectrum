using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace EspSpectrum.Core.Websocket
{
    public sealed class EspWebsocketNet : ISpectrumWebsocket, IDisplayConfigWebsocket, IDisposable
    {
        private readonly ClientWebSocket _ws = new();
        private readonly ILogger<EspWebsocketNet> _logger;
        private readonly Uri _uri;
        private CancellationTokenSource _cts = new();

        public EspWebsocketNet(IOptions<EspConfig> cfg, ILogger<EspWebsocketNet> logger)
        {
            _logger = logger;
            _uri = new Uri(cfg.Value.EspIp);
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

        public async ValueTask SendDisplayConfig(DisplayConfig config)
        {
            var json = JsonSerializer.Serialize(config);
            var buffer = Encoding.UTF8.GetBytes(json);
            await SendAsync(buffer, WebSocketMessageType.Text);
        }

        public async ValueTask SendSpectrum(Spectrum spectrum)
        {
            var packed = PackData([.. spectrum.Bands.Select(x => (int)Math.Round(x))]);
            await SendAsync(packed, WebSocketMessageType.Binary);
        }

        private async Task SendAsync(byte[] payload, WebSocketMessageType type)
        {
            if (!IsConnected())
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                await _ws.SendAsync(payload, type, true, _cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Send canceled: ESP reset?");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket send failed");
                throw;
            }
        }

        public bool IsConnected() =>
            _ws?.State == WebSocketState.Open;

        public async Task<bool> TryConnect()
        {
            _logger.LogInformation("Connecting...");

            if (IsConnected())
            {
                _logger.LogInformation("Already connected");
                return true;
            }

            try
            {
                _cts = new CancellationTokenSource();
                await _ws.ConnectAsync(_uri, _cts.Token);

                _logger.LogInformation("Connected");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect");
                return false;
            }
        }

        public Task TryConnectLoop()
        {
            _ = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
                while (await timer.WaitForNextTickAsync())
                {
                    if (!IsConnected())
                        await TryConnect();
                }
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _cts.Dispose();
                _ws?.Dispose();
            }
            catch { }
        }
    }
}
