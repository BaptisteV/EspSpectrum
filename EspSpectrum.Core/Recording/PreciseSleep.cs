using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core.Recording;

public class PreciseSleep : IPreciseSleep
{
    private TimeSpan TaskDelayOverheadTicks { get; } = TimeSpan.FromMilliseconds(5);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public async ValueTask Wait(TimeSpan waitFor, CancellationToken cancellationToken)
    {
        var targetTicks = Stopwatch.Frequency * (waitFor - TimeSpan.FromMilliseconds(1)).TotalSeconds;
        var startTicks = Stopwatch.GetTimestamp();

        void getRemaining(out TimeSpan t) => t = TimeSpan.FromMilliseconds((targetTicks - (Stopwatch.GetTimestamp() - startTicks)) * 1000.0 / Stopwatch.Frequency);

        getRemaining(out var remaining);
        while (remaining >= TimeSpan.Zero && !cancellationToken.IsCancellationRequested)
        {
            if (remaining - TaskDelayOverheadTicks >= TimeSpan.Zero) // Delay seulement si le temps restant + overhead
                await Task.Delay(remaining - TaskDelayOverheadTicks, cancellationToken);
            //Thread.Sleep((remaining - TaskDelayOverheadTicks) ?? TimeSpan.FromMilliseconds(0));
            else
                Thread.SpinWait(1000); // 100 spins is about 0.01ms on a good CPU
            getRemaining(out remaining);
        }
    }
    /*
    public ValueTask BusyWait(TimeSpan waitFor, CancellationToken cancellationToken)
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
    }*/
}