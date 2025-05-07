namespace EspSpectrum.Core;

public class EspSpectrumConfig
{
    public TimeSpan SendInterval { get; set; } = TimeSpan.FromMilliseconds(10);
    public Uri EspAdress { get; set; } = new Uri("ws://192.168.1.133:81");
}
