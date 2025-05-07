using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EspSpectrum.Core;

public static class ServiceCollectionExtensions
{
    public static void AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.Get<EspSpectrumConfig>()!;
        services.AddSingleton(config);
        services.AddTransient<IFftReader, FftReader>();
        services.AddTransient<IEspWebsocket, EspWebsocket>();
        services.AddTransient<IAudioRecorder, AudioRecorder>();
        services.AddTransient<IFftStream, FftStream>();
    }
}
