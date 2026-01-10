namespace EspSpectrum.Core.Recording;

[Flags]
public enum RunnerState
{
    None = 0,
    LoopAudioCapture = 1,
    ConnectedToEsp = 2,
    LoopReconnect = 4,
}