using System.ServiceProcess;

namespace EspSpectrum.ConfigUI;

public interface IEspSpectrumServiceMonitor
{
    (ServiceControllerStatus Status, bool IsRunning) GetStatus();
    void Restart();
}
