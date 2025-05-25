using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
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
        services.AddTransient<ISpectrumWebsocket, EspWebsocket>();
        services.AddTransient<IWaveIn, WasapiLoopbackCapture>();
        services.AddTransient<IWebsocketFactory, WebsocketFactory>();
        services.AddTransient<IFftRecorder, FftRecorder>();
        services.AddTransient<ISpectrumStream, SpectrumStream>();

        services.AddTransient<IDisplayConfigWebsocket, EspWebsocket>();

        services.AddTransient<IDisplayConfigManager, DisplayConfigWriter>();
    }
}
