using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public static class PreciseSleep
{
    public static void Wait(TimeSpan waitFor, CancellationToken cancellationToken)
    {
        var targetTicks = Stopwatch.Frequency * waitFor.TotalSeconds;
        var startTicks = Stopwatch.GetTimestamp();

        bool shouldWait() => Stopwatch.GetTimestamp() - startTicks < targetTicks;
        while (shouldWait() && !cancellationToken.IsCancellationRequested)
        {
            Thread.SpinWait(100);
        }
    }
}