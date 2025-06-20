using EspSpectrum.Core.Fft;

namespace EspSpectrum.Core.Websocket;

public interface ISpectrumWebsocket
{
    ValueTask SendSpectrum(Spectrum spectrum);
}