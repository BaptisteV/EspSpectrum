using System.ServiceProcess;

namespace EspSpectrum.ConfigUI;

public class EspSpectrumServiceMonitor : IEspSpectrumServiceMonitor
{
    private static readonly string ServiceName = "EspSpectrum";
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

    public void Restart()
    {
        var service = new ServiceController(ServiceName);
        if (service.CanStop)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, Timeout);
        }

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, Timeout);
    }

    public void Stop()
    {
        var service = new ServiceController(ServiceName);
        if (service.CanStop)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, Timeout);
        }
    }

    (ServiceControllerStatus Status, bool IsRunning) IEspSpectrumServiceMonitor.GetStatus()
    {
        var status = new ServiceController(ServiceName).Status;
        return (Status: status, IsRunning: status == ServiceControllerStatus.Running);
    }
}
