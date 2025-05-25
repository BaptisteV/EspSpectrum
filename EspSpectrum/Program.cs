using EspSpectrum.Core;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: true)
    .Build();
static IServiceProvider Configure(IConfiguration configuration)
{
    var services = new ServiceCollection();
    ConfigureServices(services, configuration);
    return services.BuildServiceProvider();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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
    services.AddCoreServices(configuration);
}

var sp = Configure(configuration);

var fftStream = sp.GetRequiredService<ISpectrumStream>();
var ws = sp.GetRequiredService<ISpectrumWebsocket>();

await foreach (var fft in fftStream.NextFft())
{
    await ws.SendSpectrum(fft.Bands);
}