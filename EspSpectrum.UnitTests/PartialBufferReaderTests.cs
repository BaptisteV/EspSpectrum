using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;

namespace EspSpectrum.UnitTests;

public class PartialBufferReaderTests
{
    [Theory(DisplayName = "Consume single read")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public void MovingProcessingOne(int frameSize)
    {
        var buffer = new PartialDataReader(
            LoggerFactory.Create(o => { }).CreateLogger("PartialDataReader"),
            sampleSize: frameSize,
            destructiveReadLength: 1);
        var res = Enumerable.Range(1, frameSize).Select(a => (float)a).ToArray();
        buffer.AddData(res);
        Assert.Equal(frameSize, buffer.ApproximateCount);

        var read = buffer.TryRead(out var buffs);

        Assert.True(read);
        Assert.Equal(frameSize - 1, buffer.ApproximateCount);
        Assert.Equal(frameSize, buffs.Count());
        var result = res.Take(frameSize);
        Assert.True(buffs.SequenceEqual(result));
    }

    [Theory(DisplayName = "Multiple buffs, number of buffers")]
    [InlineData(new float[] { 1f, 2f }, 2, 2, 1)]
    [InlineData(new float[] { 1f, 2f }, 1, 1, 2)]
    [InlineData(new float[] { 1f, 2f, 3f }, 2, 1, 2)]
    [InlineData(new float[] { 1f, 2f, 3f }, 1, 1, 3)]
    [InlineData(new float[] { 1f, 2f, 3f, 4f }, 3, 1, 2)]
    public void MovingMultipleBuffs_ExpectedCounts(float[] data, int frameSize, int destructieReadLength, int expectedBuffsCount)
    {
        var buffer = new PartialDataReader(LoggerFactory.Create(o => { }).CreateLogger("PartialDataReader"), frameSize, destructieReadLength);
        buffer.AddData(data);

        var buffs = new List<float[]>();

        var dataLeft = buffer.ApproximateCount;
        while (buffer.TryRead(out var datar))
        {
            buffs.Add(datar.ToArray());
            dataLeft -= destructieReadLength;
            Assert.Equal(dataLeft, buffer.ApproximateCount);
        }

        Assert.Equal(expectedBuffsCount, buffs.Count);
        Assert.Equal(data[0], buffs[0][0]);
        if (expectedBuffsCount > 1)
            Assert.Equal(data[destructieReadLength], buffs[1][0]);
    }

    [Theory(DisplayName = "Multiple buffs, number of buffers")]
    [InlineData(new float[] { 1f, 2f }, 2, 2, 1)]
    [InlineData(new float[] { 1f, 2f }, 1, 1, 2)]
    [InlineData(new float[] { 1f, 2f, 3f }, 2, 1, 2)]
    [InlineData(new float[] { 1f, 2f, 3f }, 1, 1, 3)]
    [InlineData(new float[] { 1f, 2f, 3f, 4f }, 3, 1, 2)]
    public void MovingMultipleBuffs_ExpectedData(float[] data, int frameSize, int destructieReadLength, int expectedBuffsCount)
    {
        var buffer = new PartialDataReader(LoggerFactory.Create(o => { }).CreateLogger("PartialDataReader"), frameSize, destructieReadLength);
        buffer.AddData(data);

        var buffs = new List<float[]>();
        while (buffer.TryRead(out var datar))
        {
            buffs.Add(datar.ToArray());
        }

        var firsts = buffs.Select(b => b[0]).ToArray();
        var expectedFirsts = new List<float>();
        for (int i = 0; i < buffs.Count; i += destructieReadLength)
        {
            expectedFirsts.Add(data[i]);
        }
        Assert.True(firsts.SequenceEqual(expectedFirsts));
    }

    public void MultipleDataIsConsumed()
    {
        var frameSize = 2;
        var buffer = new PartialDataReader(LoggerFactory.Create(o => { }).CreateLogger("PartialDataReader"), sampleSize: frameSize, destructiveReadLength: 1);
        var res = Enumerable.Range(1, frameSize).Select(a => (float)a).ToArray();
        buffer.AddData(res);
        Assert.Equal(frameSize, buffer.ApproximateCount);

    }
}
