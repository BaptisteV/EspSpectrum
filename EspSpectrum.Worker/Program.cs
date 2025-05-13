using EspSpectrum.Core;
using EspSpectrum.Worker;

var builder = Host.CreateDefaultBuilder(args);

builder.UseWindowsService(c =>
{
    c.ServiceName = "EspSpectrum";
});

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
}).ConfigureServices((hostContext, services) =>
{
    services.AddCoreServices(hostContext.Configuration);
    services.AddHostedService<Worker>();
});
var host = builder.Build();
await host.RunAsync();
