using EspSpectrum.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

static IServiceProvider Configure()
{
    var services = new ServiceCollection();
    ConfigureServices(services);
    return services.BuildServiceProvider();
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(config =>
    {
        config.AddSimpleConsole(c =>
        {
            c.TimestampFormat = "[yyyy-MM-ddTHH:mm:ss] ";
            c.SingleLine = true;
        });
        config.SetMinimumLevel(LogLevel.Information);
    });
    services.AddCoreServices();
}

var sp = Configure();

var fftStream = sp.GetRequiredService<IFftStream>();
var ws = sp.GetRequiredService<IEspWebsocket>();

await foreach (var fft in fftStream.NextFft())
{
    await ws.SendAudio(fft.Bands);
}