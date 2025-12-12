using EspSpectrum.Core;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Recording.TimingMonitoring;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AndroidMic
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
            builder.Logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss:fff ";
                options.IncludeScopes = false;
            });
            // Load embedded JSON config
            using var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("AndroidMic.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);
            builder.Services.Configure<EspConfig>(builder.Configuration);
            builder.Services.Configure<DisplayConfig>(builder.Configuration);
            builder.Services.Configure<SpectrumConfig>(builder.Configuration);

            builder.Services.AddTransient<IWebsocketFactory, WebsocketFactory>();
            builder.Services.AddTransient<ISpectrumWebsocket, EspWebsocket>();
            builder.Services.AddTransient<ITickTimingMonitor, TimingMonitor>();
            builder.Services.AddTransient<ISyncSpectrumReader, SyncSpectrumReader>();
            builder.Services.AddTransient<IDataReader, PartialDataReader>();
            builder.Services.AddTransient<IFftRecorder, AndroidFftRecorder>();
            builder.Services.AddTransient<IPreciseSleep, PreciseSleep>();
            builder.Services.AddTransient<IEspSpectrumRunner, EspSpectrumRunner>();

            return builder.Build();
        }
    }
}
