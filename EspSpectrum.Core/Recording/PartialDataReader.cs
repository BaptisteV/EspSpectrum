using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core.Recording;

public class PartialDataReader : IDataReader
{
    private readonly ConcurrentQueue<float> _queue = new();
    private readonly ILogger _logger;
    private readonly int _sampleSize;
    private readonly int _destructiveReadLength;
    private readonly int _maxQueueSize;
    private volatile int _approximateCount;

    public PartialDataReader(ILogger logger, int sampleSize, int destructiveReadLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleSize, nameof(sampleSize));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(destructiveReadLength, nameof(destructiveReadLength));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(destructiveReadLength, sampleSize, nameof(destructiveReadLength));

        _logger = logger;
        _sampleSize = sampleSize;
        _destructiveReadLength = destructiveReadLength;
        _maxQueueSize = sampleSize * 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFull() => _approximateCount >= _maxQueueSize;

    private void Dequeue(int nDequeue)
    {
        var dequeuedCount = 0;
        while (dequeuedCount < nDequeue)
        {
            if (_queue.TryDequeue(out _))
            {
                dequeuedCount++;
            }
            else
            {
                break;
            }
        }

        Interlocked.Add(ref _approximateCount, -dequeuedCount);
    }

    public void AddData(ReadOnlySpan<float> newData)
    {
        if (IsFull())
        {
            Dequeue(newData.Length);
            _logger.LogDebug("Queue was full. Dropped {DroppedLength}, {QueueLength}", newData.Length, _queue.Count);
        }
        var count = 0;
        foreach (var d in newData)
        {
            _queue.Enqueue(d);
            count++;
        }

        Interlocked.Add(ref _approximateCount, count);
    }

    public bool TryRead(out float[] data)
    {
        if (_queue.Count < _sampleSize)
        {
            data = [];
            return false;
        }

        data = _queue.Take(_sampleSize).ToArray();

        Dequeue(_destructiveReadLength);
        return true;
    }

    public List<float[]> ReadAll()
    {
        var buffs = new List<float[]>();
        while (TryRead(out var data))
        {
            buffs.Add(data);
        }
        return buffs;
    }

    public int ApproximateCount => _approximateCount;
}