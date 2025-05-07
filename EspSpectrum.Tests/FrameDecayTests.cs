using EspSpectrum.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace EspSpectrum.DisplayTests;

public class FrameDecayTests
{
    private readonly EspWebsocket _espWebsocket;
    public FrameDecayTests()
    {
        _espWebsocket = new EspWebsocket(new EspSpectrumConfig(), NullLogger<EspWebsocket>.Instance);
    }

    [Fact]
    public async Task SingleFrameShouldFadeout()
    {
        var zer = BarsGen.GetLine(0);
        await _espWebsocket.SendAudio(zer);
        await Task.Delay(1000);
        for (var i = 0; i < 50; i++)
        {
            var d = BarsGen.GetLine(8);
            await _espWebsocket.SendAudio(d);
            await Task.Delay(250);
            var un = BarsGen.GetLine(1);
            await _espWebsocket.SendAudio(un);
            await Task.Delay(250);
        }
        Assert.True(true);
    }
}
