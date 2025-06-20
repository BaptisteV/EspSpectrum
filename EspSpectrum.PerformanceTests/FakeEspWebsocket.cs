using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;

namespace EspSpectrum.PerformanceTests;

public sealed class FakeEspWebsocket : IDisplayConfigWebsocket, ISpectrumWebsocket
{
    public ValueTask SendDisplayConfig(DisplayConfig displayConfig)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SendSpectrum(Spectrum spectrum)
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }

}
