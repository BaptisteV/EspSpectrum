using Microsoft.Extensions.DependencyInjection;

namespace EspSpectrum.Core;

public static class ServiceCollectionExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IFftReader, FftReader>();
        services.AddTransient<IEspWebsocket, EspWebsocket>();
        services.AddTransient<IAudioRecorder, AudioRecorder>();
        services.AddTransient<IFftStream, FftStream>();
    }
}
