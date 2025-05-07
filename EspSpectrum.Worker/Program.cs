using EspSpectrum.Core;
using EspSpectrum.Worker;

var builder = Host.CreateDefaultBuilder(args);
builder.UseWindowsService(c =>
{
    c.ServiceName = "EspSpectrum";
});

builder.ConfigureServices((hostContext, services) =>
{
    services.AddCoreServices();
    services.AddHostedService<Worker>();
});
// Enables running as a Windows Service


var host = builder.Build();
await host.RunAsync();
