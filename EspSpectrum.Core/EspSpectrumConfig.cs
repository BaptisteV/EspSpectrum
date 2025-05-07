namespace EspSpectrum.Core;

public class EspSpectrumConfig
{
    public TimeSpan SendInterval { get; set; } = TimeSpan.FromMilliseconds(8);
    public Uri EspAdress { get; set; } = new Uri("ws://192.168.1.133:81");
}
