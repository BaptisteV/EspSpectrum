using EspSpectrum.Core;
using Microsoft.Extensions.Logging;

namespace EspSpectrum.App
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
#endif
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddTransient<IFftReader, FftReader>();
            builder.Services.AddTransient<IEspWebsocket, EspWebsocket>();
            builder.Services.AddTransient<IAudioRecorder, AudioRecorder>();
            builder.Services.AddTransient<IFftStream, FftStream>();

            return builder.Build();
        }
    }
}
