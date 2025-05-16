
namespace EspSpectrum.Core.Display
{
    public interface IDisplayConfigManager
    {
        Task<DisplayConfig> ReadConfig();
        Task UpdateConfig(Action<DisplayConfig> updateAction);
    }
}