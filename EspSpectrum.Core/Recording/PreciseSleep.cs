using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class PreciseSleep : IPreciseSleep
{
    public void Wait(TimeSpan waitFor, CancellationToken cancellationToken)
    {
        var targetTicks = Stopwatch.Frequency * waitFor.TotalSeconds;
        var startTicks = Stopwatch.GetTimestamp();

        double getRemainingMs() => (targetTicks - (Stopwatch.GetTimestamp() - startTicks)) * 1000.0 / Stopwatch.Frequency;

        while (Stopwatch.GetTimestamp() - startTicks < targetTicks && !cancellationToken.IsCancellationRequested)
        {
            // Alternance entre SpinWait et Yield pour éviter la monopolisation du CPU
            Thread.SpinWait(10);
            if (getRemainingMs() > 0.1) // Yield seulement si il reste un peu de temps
                Thread.Yield();
        }
    }
}