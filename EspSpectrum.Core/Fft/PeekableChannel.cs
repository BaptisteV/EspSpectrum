using System.Threading.Channels;

namespace EspSpectrum.Core.Fft;

public class PeekableChannel<T>
{
    private readonly Channel<T> _sourceChannel;
    private readonly Queue<T> _peekBuffer = new Queue<T>();

    public PeekableChannel(Channel<T> sourceChannel)
    {
        _sourceChannel = sourceChannel;
    }

    // Expose the underlying channel writer for input
    public ChannelWriter<T> Writer => _sourceChannel.Writer;

    /// <summary>
    /// Reads a specific number of items and consumes only a portion of them.
    /// </summary>
    /// <param name="itemsToRead">Total number of items to read</param>
    /// <param name="itemsToConsume">Number of items to consume (must be less than or equal to itemsToRead)</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>All read items (both consumed and peeked)</returns>
    public async Task<IReadOnlyList<T>> ReadPartialConsume(int itemsToRead, int itemsToConsume, CancellationToken cancellationToken = default)
    {
        if (itemsToConsume > itemsToRead)
            throw new ArgumentException("Items to consume cannot exceed items to read", nameof(itemsToConsume));

        if (itemsToRead <= 0)
            throw new ArgumentException("Items to read must be greater than zero", nameof(itemsToRead));

        if (itemsToConsume < 0)
            throw new ArgumentException("Items to consume cannot be negative", nameof(itemsToConsume));

        var result = new List<T>(itemsToRead);

        // First, use any items already in the peek buffer
        while (_peekBuffer.Count > 0 && result.Count < itemsToRead)
        {
            result.Add(_peekBuffer.Dequeue());
        }

        // Read remaining items needed from the channel
        while (result.Count < itemsToRead)
        {
            if (_sourceChannel.Reader.TryRead(out var item))
            {
                result.Add(item);
            }
        }

        // Now handle the split between consumed and peeked items
        int consumedCount = Math.Min(itemsToConsume, result.Count);
        int peekedCount = result.Count - consumedCount;

        // Re-queue the items that should be peeked but not consumed
        if (peekedCount > 0)
        {
            for (int i = consumedCount; i < result.Count; i++)
            {
                _peekBuffer.Enqueue(result[i]);
            }
        }

        return result;
    }

    // Basic read method that consumes one item
    public async ValueTask<T> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (_peekBuffer.Count > 0)
        {
            return _peekBuffer.Dequeue();
        }

        return await _sourceChannel.Reader.ReadAsync(cancellationToken);
    }

    // Basic peek method without consumption
    public async ValueTask<IReadOnlyList<T>> PeekAsync(int count, CancellationToken cancellationToken = default)
    {
        var peekedItems = await ReadPartialConsume(count, 0, cancellationToken);
        return peekedItems;
    }

    // Consume a specific number of items
    public async Task<IReadOnlyList<T>> ReadAsync(int count, CancellationToken cancellationToken = default)
    {
        return await ReadPartialConsume(count, count, cancellationToken);
    }

    // Check if the channel has data available
    public bool CanRead => _peekBuffer.Count > 0 || _sourceChannel.Reader.CanCount && _sourceChannel.Reader.Count > 0;

    // Get current buffer size
    public int BufferedCount => _peekBuffer.Count;

    public int Count => _sourceChannel.Reader.Count + _peekBuffer.Count;
}
