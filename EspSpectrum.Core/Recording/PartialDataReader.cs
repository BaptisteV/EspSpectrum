using EspSpectrum.Core.Fft;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core.Recording;

public class PartialDataReader(
    int sampleSize = FftProps.FftLength,
    int destructiveReadLength = FftProps.ReadLength) : IDataReader
{
    private readonly ConcurrentQueue<float> _queue = new();
    private readonly int _sampleSize = sampleSize;
    private readonly int _destructiveReadLength = destructiveReadLength;
    private readonly int _maxQueueSize = sampleSize * 3;
    private volatile int _queueSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFull() => _queueSize >= _maxQueueSize;

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

        Interlocked.Add(ref _queueSize, -dequeuedCount);
    }

    public void AddData(ReadOnlySpan<float> newData)
    {
        if (IsFull())
        {
            var dequeuedCount = _queueSize - _maxQueueSize + newData.Length;
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

        Interlocked.Add(ref _queueSize, count);
    }

    /// <summary>
    /// Attempts to read an audio frame of the specified size from the queue.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>True if we successfully read audio data</returns>
    public bool TryReadAudioFrame(Span<float> data)
    {
        if (_queue.Count < _sampleSize)
        {
            return false;
        }

        _queue.Take(_sampleSize).ToArray().AsSpan().CopyTo(data);

        Dequeue(_destructiveReadLength);
        return true;
    }

    public int Count() => _queueSize;
}