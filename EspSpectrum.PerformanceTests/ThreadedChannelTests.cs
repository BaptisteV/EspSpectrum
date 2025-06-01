using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using System.Threading.Channels;

namespace EspSpectrum.PerformanceTests;


[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class ThreadedChannelTests
{
    private Channel<float> _dataChannel;
    private FeederThread _t;

    [GlobalSetup]
    public async Task Setup()
    {
        _dataChannel = Channel.CreateUnbounded<float>();
        _t = new FeederThread(_dataChannel, TimeSpan.FromMilliseconds(1), 8820);
        await _t.Start(true);
    }

    [Benchmark]
    public async Task ChannelTest()
    {
        var channel = new PeekableChannel<float>(_dataChannel);
        var itemsToRead = FftProps.FftLength;
        var itemsToConsume = FftProps.ReadLength;
        _ = await channel.ReadPartialConsume(itemsToRead, itemsToConsume);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _t.Stop();
    }
}
