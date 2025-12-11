using EspSpectrum.Core;
using EspSpectrum.Worker;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHostOptions(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});

builder.UseWindowsService(c =>
{
    c.ServiceName = "EspSpectrum";
});

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .SetBasePath(AppContext.BaseDirectory);
}).ConfigureServices((hostContext, services) =>
{
    services.AddCoreServices(hostContext.Configuration);
    services.AddHostedService<Worker>();
}).ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss:fff ";
        options.IncludeScopes = false;
    });
});

var host = builder.Build();
await host.RunAsync();
