namespace EspSpectrum.Core.Recording;

public interface IPreciseSleep
{
    void Wait(TimeSpan waitFor, CancellationToken cancellationToken);
}