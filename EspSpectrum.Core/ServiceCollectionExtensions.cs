using EspSpectrum.Core.Display;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EspSpectrum.Core;

public static class ServiceCollectionExtensions
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EspConfig>(configuration);
        services.Configure<DisplayConfig>(configuration);
        services.AddTransient<IFftReader, FftReader>();
        services.AddTransient<IEspWebsocket, EspWebsocket>();
        services.AddTransient<IAudioRecorder, AudioRecorder>();
        services.AddTransient<IFftStream, FftStream>();

        services.AddTransient<IDisplayConfigWebsocket, DisplayConfigWebsocket>();
        services.AddSingleton<IDisplayConfigChangeHandler, DisplayConfigChangeHandler>();
    }
}
