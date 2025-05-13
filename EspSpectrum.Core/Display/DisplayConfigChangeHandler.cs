using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EspSpectrum.Core.Display;

public class DisplayConfigChangeHandler : IDisplayConfigChangeHandler
{
    private readonly ILogger<DisplayConfigChangeHandler> _logger;
    private DisplayConfig _config;
    private readonly IDisplayConfigWebsocket _ws;

    public DisplayConfigChangeHandler(
        IOptionsMonitor<DisplayConfig> optionsMonitor,
        IDisplayConfigWebsocket ws,
        ILogger<DisplayConfigChangeHandler> logger)
    {
        _logger = logger;
        _ws = ws;
        _config = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(async newConfig =>
        {
            if (newConfig == _config)
                return;
            _logger.LogInformation("DisplayConfig changed!");
            _config = newConfig;
            await _ws.Send(_config);
        });
        _ = Task.Run(async () =>
        {
            try
            {
                await _ws.Send(_config);
            }
            catch { }
        });
    }
}
