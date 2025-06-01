using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using System.Threading.Channels;

namespace EspSpectrum.PerformanceTests;


[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class ReadChannelTests
{
    [Benchmark]
    public async Task ChannelTest()
    {
        var dataChannel = Channel.CreateUnbounded<float>();
        await FeederThread.FeedData(dataChannel, FftProps.FftLength);

        var channel = new PeekableChannel<float>(dataChannel);
        var itemsToRead = FftProps.FftLength;
        var itemsToConsume = FftProps.ReadLength;

        _ = await channel.ReadPartialConsume(itemsToRead, itemsToConsume);

        await FeederThread.FeedData(dataChannel, FftProps.FftLength);
        _ = await channel.ReadPartialConsume(itemsToRead, itemsToConsume);
    }
}
