using EspSpectrum.Core.Display;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EspSpectrum.ConfigUI;

internal static class Program
{
    internal static IServiceProvider ServiceProvider { get; private set; }

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
                services.AddTransient<IDisplayConfigManager, DisplayConfigWriter>(
                    (sp) =>
                        new DisplayConfigWriter(sp.GetRequiredService<ILogger<DisplayConfigWriter>>(), EspSpectrumConfigForm.DefaultAppsettings));

                services.AddTransient<EspSpectrumConfigForm>();
                services.AddTransient<IEspSpectrumServiceMonitor, EspSpectrumServiceMonitor>();
            });
    }
}