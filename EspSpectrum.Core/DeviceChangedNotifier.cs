using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace EspSpectrum.Core;

public class DeviceChangedNotifier(ILogger logger, IFftReader reader) : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
{
    private readonly IFftReader _reader = reader;

    public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
    {
        logger.LogInformation("Default output device changed. Restarting");
        _reader.Restart();
    }

    public void OnDeviceAdded(string deviceId)
    {
    }

    public void OnDeviceRemoved(string deviceId)
    {
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        //_logger.LogWarning("OnDeviceStateChanged Device Id -->{DeviceId} : Device State {DeviceState}", deviceId, newState);
    }

    public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
    {
        //_logger.LogInformation("OnPropertyValueChanged: propertyId --> {1}", propertyKey.propertyId.ToString());
    }

}
