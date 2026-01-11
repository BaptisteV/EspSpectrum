using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EspSpectrum.Core.Recording.TimingMonitoring;

public class ExecutionMonitor(ILogger logger)
{
    private static readonly int HISTO_SIZE = 200;
    private readonly ConcurrentQueue<double> _mesurements = [];

    public void NotifytLoopDone(double doneAfter)
    {
        _mesurements.Enqueue(doneAfter);
        if (_mesurements.Count > HISTO_SIZE)
            _mesurements.TryDequeue(out var _);
    }


    private void LogSummary()
    {
        // Need at least 2 measurements to compute diffs
        if (_mesurements.Count <= 1)
            return;

        var mesurements = _mesurements.ToArray();
        var count = mesurements.Length;
        var average = mesurements.Average();
        var min = mesurements.Min();
        var max = mesurements.Max();
        var dev = mesurements.StandardDeviation();

        logger.LogInformation("ExecutionMonitor: Count={Count}\t" +
            "Average={Average:n2}ms\t" +
            "Min={Min:n2}ms\t" +
            "Max={Max:n2}ms\t" +
            "Standard deviation={StandardDeviation:n2}", count, average, min, max, dev);
    }

    public void StartLogLoop()
    {
        Task.Factory.StartNew(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (await timer.WaitForNextTickAsync())
            {
                LogSummary();
            }
        }, TaskCreationOptions.LongRunning);
    }
}