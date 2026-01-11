namespace EspSpectrum.Core.Recording;

public interface IPreciseSleep
{
    ValueTask Wait(TimeSpan waitFor, CancellationToken cancellationToken);
    void WaitSync(TimeSpan waitFor);
}