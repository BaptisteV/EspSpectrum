using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Websocket.Client;

namespace EspSpectrum.Core.Display;

public class DisplayConfigWebsocket : IWebsocketDisplay
{
    private readonly ILogger<DisplayConfigWebsocket> _logger;
    private readonly WebsocketClient _wsClient;
    private bool _starting = false;

    public DisplayConfigWebsocket(
        ILogger<DisplayConfigWebsocket> logger,
        IOptions<EspConfig> config
        )
    {
        _logger = logger;
        _wsClient = config.Value.GetWebsocketClient(_logger);
    }

    public async Task Send(DisplayConfig displayConfig)
    {
        if (!_wsClient.IsRunning && !_starting)
        {
            _starting = true;
            _logger.LogInformation("Connecting...");
            await _wsClient.Start();
            _logger.LogInformation("Connected");
            _starting = false;
        }
        var jsonString = JsonSerializer.Serialize(displayConfig);
        await _wsClient.SendInstant(jsonString);
        _logger.LogInformation("Sent {Json}", jsonString);
    }
}
