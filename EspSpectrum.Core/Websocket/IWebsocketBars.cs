
namespace EspSpectrum.Core;

public interface IWebsocketBars
{
    Task SendAudio(int[] audio);
}