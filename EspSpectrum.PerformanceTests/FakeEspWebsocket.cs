using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;

namespace EspSpectrum.PerformanceTests;

public sealed class FakeEspWebsocket : IDisplayConfigWebsocket, ISpectrumWebsocket
{
    public ValueTask SendDisplayConfig(DisplayConfig displayConfig, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask SendSpectrum(Spectrum spectrum, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }

    public Task ReconnectLoop(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<bool> TryConnect(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public bool IsConnected()
    {
        return true;
    }
}
