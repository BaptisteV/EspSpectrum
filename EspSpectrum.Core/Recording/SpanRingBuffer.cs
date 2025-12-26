using EspSpectrum.Core.Fft;
using System.Runtime.CompilerServices;

namespace EspSpectrum.Core.Recording;


public sealed class SpanRingBuffer : IDataReader
{
    private readonly float[] _buffer;
    private readonly int _sampleSize;
    private readonly int _destructiveRead;
    private int _writeIndex;
    private int _count;

    public SpanRingBuffer(int sampleSize = FftProps.FftLength, int destructiveReadLength = FftProps.ReadLength)
    {
        _sampleSize = sampleSize;
        _destructiveRead = destructiveReadLength;
        _buffer = new float[sampleSize * 3];   // capacity
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Count() => _count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddData(ReadOnlySpan<float> newData)
    {
        for (int i = 0; i < newData.Length; i++)
        {
            _buffer[_writeIndex] = newData[i];
            _writeIndex++;

            if (_writeIndex == _buffer.Length)
                _writeIndex = 0;

            if (_count < _buffer.Length)
                _count++;
        }
    }

    public bool TryReadAudioFrame(Span<float> dest)
    {
        if (_count < _sampleSize)
            return false;

        // Compute start index of oldest data
        int start = (_writeIndex - _count + _buffer.Length) % _buffer.Length;

        // Copy _sampleSize floats into dest (span)
        int firstCopy = Math.Min(_sampleSize, _buffer.Length - start);
        _buffer.AsSpan(start, firstCopy).CopyTo(dest);

        if (firstCopy < _sampleSize)
            _buffer.AsSpan(0, _sampleSize - firstCopy).CopyTo(dest[firstCopy..]);

        // Destructive read — drop old data
        _count -= _destructiveRead;
        if (_count < 0) _count = 0;

        return true;
    }
}
