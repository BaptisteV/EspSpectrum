namespace EspSpectrum.Core.Websocket;

public interface ISpectrumWebsocket
{
    Task SendSpectrum(double[] bands);
}