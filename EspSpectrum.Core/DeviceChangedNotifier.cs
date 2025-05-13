using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace EspSpectrum.Core;

public class DeviceChangedNotifier(ILogger logger, IFftReader reader) : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
{
    private readonly IFftReader _reader = reader;

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        logger.LogInformation("Default output device changed. Restarting");
        _reader.Restart();
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
    }

    public void OnDeviceAdded(string pwstrDeviceId)
    {
    }

    public void OnDeviceRemoved(string deviceId)
    {
    }

    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
    {
    }
}
