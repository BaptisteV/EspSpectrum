namespace EspSpectrum.Core.Fft;

public class RateLimiter
{
    private readonly TimeSpan _interval;
    private long _lastExecutionTicks = 0;
    private int _isRunning = 0;

    public RateLimiter(int milliseconds)
    {
        _interval = TimeSpan.FromMilliseconds(milliseconds);
    }

    /// <summary>
    /// Executes async action only if allowed. If invoked too soon, call is discarded.
    /// </summary>
    public async Task TryExecuteAsync(Func<Task> action)
    {
        long now = DateTime.UtcNow.Ticks;
        long last = Interlocked.Read(ref _lastExecutionTicks);

        if (new TimeSpan(now - last) < _interval)
            return;  // 🔥 discard

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return;  // 🔥 discard if another execution is already running

        try
        {
            Interlocked.Exchange(ref _lastExecutionTicks, now);
            await action();
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }

    /// <summary>
    /// Sync overload
    /// </summary>
    public Task TryExecute(Action action) =>
        TryExecuteAsync(() => { action(); return Task.CompletedTask; });
}
