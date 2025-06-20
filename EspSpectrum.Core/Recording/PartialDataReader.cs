using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core.Recording;

public class PartialDataReader(
    ILogger<PartialDataReader> logger,
    int sampleSize = FftProps.FftLength,
    int destructiveReadLength = FftProps.ReadLength) : IDataReader
{
    private readonly ConcurrentQueue<float> _queue = new();
    private readonly ILogger<PartialDataReader> _logger = logger;
    private readonly int _sampleSize = sampleSize;
    private readonly int _destructiveReadLength = destructiveReadLength;
    private readonly int _maxQueueSize = sampleSize * 2;
    private volatile int _approximateCount;

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
            var dequeuedCount = _approximateCount - _maxQueueSize + newData.Length;
            Dequeue(dequeuedCount);
        }

        EnqueueData(newData);
    }

    private void EnqueueData(ReadOnlySpan<float> newData)
    {
        var count = 0;
        foreach (var d in newData)
        {
            _queue.Enqueue(d);
            count++;
        }

        Interlocked.Add(ref _approximateCount, count);
    }

    public bool TryReadAudioFrame(Span<float> data)
    {
        if (_queue.Count < _sampleSize)
        {
            return false;
        }

        var copied = 0;
        foreach (var value in _queue.Take(_sampleSize))
        {
            data[copied++] = value;
        }

        Dequeue(_destructiveReadLength);
        return true;
    }

    public bool TryReadAudioFrame(out float[] data)
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

    public int Count() => _approximateCount;
}