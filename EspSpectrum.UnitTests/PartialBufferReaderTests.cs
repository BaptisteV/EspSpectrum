using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public class PartialBufferReaderTests(ITestOutputHelper testOutputHelper) : BaseTests(testOutputHelper)
{
    [Fact]
    public void ReadFull()
    {
        var dr = new PartialDataReader(NullLogger<PartialDataReader>.Instance, 1000, 1000);
        var count = 1000;
        var data = new float[count];
        for (var i = 0; i < count; i++)
        {
            data[i] = i;
        }
        dr.AddData(data);
        var result = new float[count];
        var firstRead = dr.TryReadAudioFrame(result);
        Assert.True(firstRead);
        var expectedResult = Enumerable.Range(0, count).Select(d => (float)d).ToList();
        Assert.True(expectedResult.SequenceEqual(result.ToArray()));
        var emptyResult = new float[count];
        var secondRead = dr.TryReadAudioFrame(emptyResult);
        Assert.False(secondRead);
    }

    [Theory(DisplayName = "Consume single secondRead")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public void MovingProcessingOne(int frameSize)
    {
        var buffer = new PartialDataReader(
            NullLogger<PartialDataReader>.Instance,
            sampleSize: frameSize,
            destructiveReadLength: 1);
        var res = Enumerable.Range(1, frameSize).Select(a => (float)a).ToArray();
        buffer.AddData(res);
        Assert.Equal(frameSize, buffer.Count());

        var buffs = new float[frameSize];
        var read = buffer.TryReadAudioFrame(buffs);

        Assert.True(read);
        Assert.Equal(frameSize - 1, buffer.Count());
        Assert.Equal(frameSize, buffs.Length);
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
        var buffer = new PartialDataReader(NullLogger<PartialDataReader>.Instance, frameSize, destructieReadLength);
        buffer.AddData(data);

        var buffs = new List<float[]>();

        var dataLeft = buffer.Count();
        var datar = new float[frameSize];
        while (buffer.TryReadAudioFrame(datar))
        {
            buffs.Add(datar.ToArray());
            dataLeft -= destructieReadLength;
            Assert.Equal(dataLeft, buffer.Count());
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
        var buffer = new PartialDataReader(NullLogger<PartialDataReader>.Instance, frameSize, destructieReadLength);
        buffer.AddData(data);

        var buffs = new List<float[]>();
        var datar = new float[frameSize];
        while (buffer.TryReadAudioFrame(datar))
        {
            buffs.Add(datar.ToArray());
        }

        Assert.Equal(expectedBuffsCount, buffs.Count);
        var firsts = buffs.Select(b => b[0]).ToArray();
        var expectedFirsts = new List<float>();
        for (int i = 0; i < buffs.Count; i += destructieReadLength)
        {
            expectedFirsts.Add(data[i]);
        }
        Assert.True(firsts.SequenceEqual(expectedFirsts));
    }
}
