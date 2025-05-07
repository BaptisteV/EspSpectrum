using EspSpectrum.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace EspSpectrum.DisplayTests;

public class SendSlowTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;
    private readonly EspWebsocket _espWebsocket = new EspWebsocket(new EspSpectrumConfig(), NullLogger<EspWebsocket>.Instance);

    [Fact]
    public async Task UpDown1Sec()
    {
        for (var i = 0; i <= 8; i++)
        {
            var d = BarsGen.GetLine(i);
            await _espWebsocket.SendAudio(d);
            _output.WriteLine($"Sent {i}");
            await Task.Delay(250);
        }

        for (var i = 8; i >= 0; i--)
        {
            var d = BarsGen.GetLine(i);
            await _espWebsocket.SendAudio(d);
            _output.WriteLine($"Sent {i}");
            await Task.Delay(250);
        }
        Assert.True(true);
    }

    [Theory]
    [InlineData(1000)]
    public async Task DownSlow(int ms)
    {
        for (var n = 0; n <= 5; n++)
        {
            for (var i = 8; i >= 0; i -= 2)
            {
                var d = BarsGen.GetLine(i);
                await _espWebsocket.SendAudio(d);
                _output.WriteLine($"Sent {i}");
                await Task.Delay(ms);
            }
        }
        Assert.True(true);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(50)]
    [InlineData(40)]
    [InlineData(30)]
    [InlineData(20)]
    [InlineData(10)]
    public async Task DownFast(int ms)
    {
        for (var n = 0; n <= 10; n++)
        {
            for (var i = 8; i >= 0; i -= 2)
            {
                var d = BarsGen.GetLine(i);
                await _espWebsocket.SendAudio(d);
                _output.WriteLine($"Sent {i}");
                await Task.Delay(ms);
            }
        }
        Assert.True(true);
    }
}
