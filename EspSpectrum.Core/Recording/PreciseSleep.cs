using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public class PreciseSleep : IPreciseSleep
{
    public ValueTask Wait(TimeSpan waitFor, CancellationToken cancellationToken)
    {
        var targetTicks = Stopwatch.Frequency * waitFor.TotalSeconds;
        var startTicks = Stopwatch.GetTimestamp();

        double getRemainingMs() => (targetTicks - (Stopwatch.GetTimestamp() - startTicks)) * 1000.0 / Stopwatch.Frequency;

        while (Stopwatch.GetTimestamp() - startTicks < targetTicks && !cancellationToken.IsCancellationRequested)
        {
            // Alternance entre SpinWait et Yield pour éviter la monopolisation du CPU
            Thread.SpinWait(100); // 100 spins is about 0.01ms on a good CPU
            if (getRemainingMs() > 0.1) // Yield seulement si il reste un peu de temps
                Thread.Yield();
        }
        return ValueTask.CompletedTask;
    }
}