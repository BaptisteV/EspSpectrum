namespace EspSpectrum.Core.Display;

public interface IAppsettingsManager
{
    Task<DisplayConfig> ReadConfig();
    Task UpdateConfig(Action<DisplayConfig> updateConfig);
}