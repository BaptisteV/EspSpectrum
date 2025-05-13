using EspSpectrum.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace EspSpectrum.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).SetBasePath(AppContext.BaseDirectory);
            builder.Services.AddCoreServices(builder.Configuration);

            return builder.Build();
        }
    }
}
