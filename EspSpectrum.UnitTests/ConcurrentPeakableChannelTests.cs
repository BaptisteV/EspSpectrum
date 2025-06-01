using EspSpectrum.Core.Fft;
using EspSpectrum.PerformanceTests;
using System.Threading.Channels;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public class ConcurrentPeakableChannelTests : BaseTests
{
    private readonly Channel<float> _dataChannel;
    private readonly PeekableChannel<float> _channel;

    public ConcurrentPeakableChannelTests(ITestOutputHelper output) : base(output)
    {
        _dataChannel = Channel.CreateUnbounded<float>();
        _channel = new PeekableChannel<float>(_dataChannel);
    }

    [Fact]
    public async Task Overfeed()
    {
        var t = new FeederThread(_dataChannel, TimeSpan.FromMilliseconds(100), FftProps.FftLength * 2);
        await t.Start(true);

        var fft = await _channel.ReadPartialConsume(FftProps.FftLength, FftProps.ReadLength);

        Assert.True(fft.Count == FftProps.FftLength);
        t.Stop();
    }


    [Fact]
    public async Task Underfeed()
    {
        var t = new FeederThread(_dataChannel, TimeSpan.FromMilliseconds(1), 10);
        await t.Start(false);

        var fft = await _channel.ReadPartialConsume(1000, 1000);

        Assert.True(fft.Count == 1000);
        t.Stop();
    }
}
