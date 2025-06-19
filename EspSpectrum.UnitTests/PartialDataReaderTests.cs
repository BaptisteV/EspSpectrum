using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public class PartialDataReaderTests : BaseTests
{
    public PartialDataReaderTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task ReadFull()
    {
        var dr = new PartialDataReader(NullLogger.Instance, 1000, 1000);
        var count = 1000;
        var data = new float[count];
        for (var i = 0; i < count; i++)
        {
            data[i] = i;
        }
        dr.AddData(data);

        dr.TryRead(out var result);
        var expectedResult = Enumerable.Range(0, count).Select(d => (float)d).ToList();
        Assert.True(expectedResult.SequenceEqual(result));
        dr.TryRead(out var emptyResult);
        Assert.Empty(emptyResult);
    }
    /*
    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task ReadHalf(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await _dataChannel.Writer.WriteAsync(i);
        }

        var result = await _channel.ReadPartialConsume(count, count / 2);

        var expectedResult = Enumerable.Range(0, count);
        Assert.True(expectedResult.SequenceEqual(result));
        Assert.Equal(count / 2, _channel.Count());
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(FftProps.FftLength, FftProps.ReadLength)]
    [InlineData(100_000, 50_000)]
    public async Task ReadTwice(int totalCount, int consumeCount)
    {
        // Write 0, 1, 2
        for (int i = 0; i < totalCount + consumeCount; i++)
        {
            await _dataChannel.Writer.WriteAsync(i);
        }

        Assert.Equal(totalCount + consumeCount, _channel.Count());
        var result = await _channel.ReadPartialConsume(totalCount, consumeCount);
        Assert.Equal(totalCount, _channel.Count());
        var expectedResult = Enumerable.Range(0, totalCount);
        Assert.True(expectedResult.SequenceEqual(result));

        var secondResult = await _channel.ReadPartialConsume(totalCount, consumeCount);
        Assert.Equal(totalCount - consumeCount, _channel.Count());
        var expectedSecondResult = Enumerable.Range(consumeCount, totalCount);
        Assert.True(expectedSecondResult.SequenceEqual(secondResult));
    }*/
}
