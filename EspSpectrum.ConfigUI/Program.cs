using EspSpectrum.Core.Display;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace EspSpectrum.ConfigUI;

internal static class Program
{
    internal static IServiceProvider ServiceProvider { get; private set; } = default!;

    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        Application.Run(ServiceProvider.GetRequiredService<EspSpectrumConfigForm>());
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IAppsettingsManager, AppsettingsManager>(_ =>
                {
                    const string DefaultAppsettings = @"C:\Users\Bapt\Desktop\FFT_Publish\bin\appsettings.json";
                    return new AppsettingsManager(DefaultAppsettings);
                });

                services.AddTransient<EspSpectrumConfigForm>();
                services.AddTransient<IEspSpectrumServiceMonitor, EspSpectrumServiceMonitor>();
            });
    }
}