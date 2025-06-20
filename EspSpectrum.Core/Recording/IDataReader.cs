namespace EspSpectrum.Core.Recording;

/// <summary>
/// Handles audio data reading.
/// </summary>
public interface IDataReader
{
    /// <summary>
    /// Returns the number of items in the queue.
    /// </summary>
    /// <returns></returns>
    int Count();

    /// <summary>
    /// Adds new data to the queue. If the queue is full, it will dequeue enough items to make space for the new data.
    /// </summary>
    /// <param name="newData">New data</param>
    void AddData(ReadOnlySpan<float> newData);

    /// <summary>
    /// Attempts to read an audio frame of the specified size from the queue.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>True if we successfully read audio data</returns>
    bool TryReadAudioFrame(Span<float> data);
}