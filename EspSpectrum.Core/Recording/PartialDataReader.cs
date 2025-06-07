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
        /*
        if (IsFull())
        {
            _logger.LogDebug("Queue is full, dropping new data");
            return;
        }

        for (int i = 0; i < newData.Length; i++)
        {
            _queue.Enqueue(newData[i]);
        }

        Interlocked.Add(ref _approximateCount, newData.Length);*/
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
/*
public class PartialDataReader<T> : IDataReader<T>
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly ILogger _logger;
    private readonly int _sampleSize;
    private readonly int _destructiveReadLength;
    private volatile int _approximateCount;

    public PartialDataReader(ILogger logger, int sampleSize, int destructiveReadLength)
    {
        if (sampleSize <= 0)
            throw new ArgumentException("Frame size must be positive", nameof(sampleSize));
        if (destructiveReadLength <= 0)
            throw new ArgumentException("Destructive read length must be positive", nameof(destructiveReadLength));
        if (destructiveReadLength > sampleSize)
            throw new ArgumentException("Destructive read length cannot exceed frame size", nameof(destructiveReadLength));
        _logger = logger;
        _sampleSize = sampleSize;
        _destructiveReadLength = destructiveReadLength;
    }

    private bool IsFull() => _queue.Count >= _sampleSize * 8;

    public void AddData(IEnumerable<T> newData)
    {
        if (IsFull())
        {
            _logger.LogDebug("Queue is full, dropping new data");
            return;
        }

        var count = 0;
        foreach (var d in newData)
        {
            _queue.Enqueue(d);
            count++;
        }

        Interlocked.Add(ref _approximateCount, count);
    }

    public bool TryRead(out IEnumerable<T> data)
    {
        // Quick check if we might have enough data
        if (_queue.Count < _sampleSize)
        {
            data = Enumerable.Empty<T>();
            return false;
        }

        var result = new T[_sampleSize];
        var queueArray = _queue.Take(_sampleSize).ToArray();
        // Read sampleSize items
        for (int i = 0; i < _sampleSize; i++)
        {
            result[i] = queueArray[i];
        }

        var dequeuedCount = 0;
        while (dequeuedCount < _destructiveReadLength)
        {
            if (_queue.TryDequeue(out var _))
            {
                dequeuedCount++;
            }
            else
            {
                _logger.LogWarning("Failed to dequeue");
            }
        }

        Interlocked.Add(ref _approximateCount, -dequeuedCount);
        data = result;
        return true;
    }

    public List<T[]> ReadAll()
    {
        var buffs = new List<T[]>();
        while (TryRead(out var datar))
        {
            buffs.Add(datar.ToArray());
        }
        return buffs;
    }

    public int ApproximateCount => _queue.Count;
}*/