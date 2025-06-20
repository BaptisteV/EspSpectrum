namespace EspSpectrum.Core.Display;

/// <summary>
/// Reads / update DisplayConfig in appsettings.json
/// </summary>
public interface IAppsettingsManager
{
    /// <summary>
    /// Reads DisplayConfig from the JSON file
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">appsettings.json not found</exception>
    Task<DisplayConfig> ReadConfig();

    /// <summary>
    /// Selectively updates DisplayConfig properties in the JSON file
    /// </summary>
    /// <param name="updateConfig">Set DisplayConfig</param>
    /// <returns></returns>
    Task UpdateConfig(Action<DisplayConfig> updateConfig);
}