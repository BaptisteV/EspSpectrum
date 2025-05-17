using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;

namespace EspSpectrum.Core;

public static class ServiceCollectionExtensions
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EspConfig>(configuration);
        services.Configure<DisplayConfig>(configuration);
        services.AddTransient<IWebsocketBars, EspWebsocket>();
        services.AddTransient<IWaveIn, WasapiLoopbackCapture>();
        services.AddTransient<IFftRecorder, FftRecorder>();
        services.AddTransient<IFftStream, FftStream>();

        services.AddTransient<IWebsocketDisplay, DisplayConfigWebsocket>();

        services.AddTransient<IDisplayConfigManager, DisplayConfigWriter>();
    }
}
