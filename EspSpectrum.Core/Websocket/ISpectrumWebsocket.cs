namespace EspSpectrum.Core.Websocket;

public interface ISpectrumWebsocket
{
    ValueTask SendSpectrum(double[] bands);
}