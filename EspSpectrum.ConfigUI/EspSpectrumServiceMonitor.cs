using System.ServiceProcess;

namespace EspSpectrum.ConfigUI;

public class EspSpectrumServiceMonitor : IEspSpectrumServiceMonitor
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    public void Restart()
    {
        var service = new ServiceController("EspSpectrum");
        if (service.CanStop)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, Timeout);
        }

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, Timeout);
    }

    (ServiceControllerStatus Status, bool IsRunning) IEspSpectrumServiceMonitor.GetStatus()
    {
        var status = new ServiceController("EspSpectrum").Status;
        return (Status: status, IsRunning: status == ServiceControllerStatus.Running);
    }
}
