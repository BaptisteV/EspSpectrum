using System.Diagnostics;

namespace EspSpectrum.Core.Recording;

public static class PreciseSleep
{
    public static void Wait(TimeSpan waitFor)
    {
        var targetTicks = Stopwatch.Frequency * waitFor.TotalSeconds;
        var startTicks = Stopwatch.GetTimestamp();

        while (Stopwatch.GetTimestamp() - startTicks < targetTicks)
        {
            Thread.SpinWait(10);
        }
    }
}