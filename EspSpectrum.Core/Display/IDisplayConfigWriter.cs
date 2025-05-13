
namespace EspSpectrum.Core.Display
{
    public interface IDisplayConfigWriter
    {
        Task UpdateConfig(Action<DisplayConfig> updateAction);
    }
}