using System.Threading.Channels;

namespace EspSpectrum.Core.Fft;

public class PeekableChannel<T>(Channel<T> sourceChannel)
{
    private readonly Channel<T> _sourceChannel = sourceChannel;
    private readonly Channel<T> _leftovers = Channel.CreateUnbounded<T>();

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
        while (_leftovers.Reader.Count > 0 && result.Count < itemsToRead)
        {
            result.Add(await _leftovers.Reader.ReadAsync());
        }

        // Read remaining items needed from the channel
        while (result.Count < itemsToRead)
        {
            result.Add(await _sourceChannel.Reader.ReadAsync(cancellationToken));
        }

        // Now handle the split between consumed and peeked items
        int consumedCount = Math.Min(itemsToConsume, result.Count);
        int peekedCount = result.Count - consumedCount;

        // Re-queue the items that should be peeked but not consumed
        if (peekedCount > 0)
        {
            for (int i = consumedCount; i < result.Count; i++)
            {
                await _leftovers.Writer.WriteAsync(result[i]);
            }
        }

        return result;
    }

    public int Count() => _sourceChannel.Reader.Count + _leftovers.Reader.Count;
}
