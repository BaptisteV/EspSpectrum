using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;

namespace EspSpectrum.Core;

/// <summary>
/// Extension methods for IServiceCollection to register core services for the EspSpectrum application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core services for the EspSpectrum application.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EspConfig>(configuration);
        services.Configure<DisplayConfig>(configuration);
        services.Configure<SpectrumConfig>(configuration);

        services.AddTransient<IWebsocketFactory, WebsocketFactory>();
        services.AddTransient<ISpectrumWebsocket, EspWebsocket>();
        services.AddTransient<IDisplayConfigWebsocket, EspWebsocket>();
        services.AddTransient<IWaveIn, WasapiLoopbackCapture>();
        services.AddTransient<IDataReader, PartialDataReader>();
        services.AddTransient<IFftRecorder, FftRecorder>();
        services.AddTransient<ISyncSpectrumReader, SyncSpectrumReader>();
        services.AddTransient<IPreciseSleep, PreciseSleep>();
        services.AddTransient<ITickTimingMonitor, TimingMonitor>();
        services.AddTransient<IEspSpectrumRunner, EspSpectrumRunner>();
    }
}
