
namespace EspSpectrum.Core;

public interface IEspWebsocket
{
    Task SendAudio(int[] audio);
}