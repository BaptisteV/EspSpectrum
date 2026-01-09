using AndroidMic.Platforms;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System.Reflection;

namespace AndroidMic;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureEssentials(
                essentials =>
                {
                    essentials.UseVersionTracking();
                })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#if DEBUG
        //builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
        // Load embedded JSON config
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("AndroidMic.appsettings.json")
                ?? throw new InvalidOperationException("Failed to load embedded configuration file.");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);
        builder.Services.Configure<EspConfig>(builder.Configuration);
        builder.Services.Configure<DisplayConfig>(builder.Configuration);
        builder.Services.Configure<SpectrumConfig>(builder.Configuration);

        //builder.Services.AddSingleton<ISpectrumWebsocket, EspWebsocket>();
        builder.Services.AddSingleton<ISpectrumWebsocket>(sp =>
        {
            return new EspWebsocketNet(sp.GetRequiredService<IOptions<EspConfig>>(), sp.GetRequiredService<ILogger<EspWebsocketNet>>());
        });

        builder.Services.AddSingleton<ITickTimingMonitor, AsyncTimingMonitor>();
        builder.Services.AddSingleton<ISyncSpectrumReader, SyncSpectrumReader>();
        builder.Services.AddSingleton<IPreciseSleep, PreciseSleep>();
        builder.Services.AddSingleton<IDataReader, PartialDataReader>();
        builder.Services.AddSingleton<IEspSpectrumRunner, EspSpectrumRunner>();

        builder.Services.AddSingleton<IWaveIn, WasapiLoopbackCapture>();
        builder.Services.AddSingleton<IFftRecorder, PlatformFftRecorder>();


        return builder.Build();
    }
}
