namespace EspSpectrum.Core.Fft;

public class RateThrottle
{
    private readonly TimeSpan _interval;
    private long _lastExecutionTicks = 0;
    private int _isRunning = 0;

    public RateThrottle(TimeSpan interval)
    {
        _interval = interval;
    }

    /// <summary>
    /// Executes action only if not throttled. If invoked too soon, call is discarded.
    /// </summary>
    public bool TryExecute(Action action)
    {
        long now = DateTime.UtcNow.Ticks;
        long last = Interlocked.Read(ref _lastExecutionTicks);

        if (TimeSpan.FromTicks(now - last) < _interval)
            return false;  // Discard

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return false;  // Discard if another execution is already running

        try
        {
            Interlocked.Exchange(ref _lastExecutionTicks, now);
            action();
            return true;
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }
}
