using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace EspSpectrum.Core.Recording;

public class DeviceChangedNotifier(ILogger logger, IFftRecorder reader) : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
{
    private readonly IFftRecorder _recorder = reader;

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        logger.LogInformation("Default output device changed. Restarting");
        _recorder.Restart();
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
