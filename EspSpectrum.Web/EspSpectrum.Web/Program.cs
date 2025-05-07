using EspSpectrum.Core;
using EspSpectrum.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<IFftReader, FftReader>();
builder.Services.AddSingleton<IFftStream, FftStream>();
builder.Services.AddSingleton<IEspWebsocket, EspWebsocket>();
builder.Services.AddSingleton<IAudioRecorder, AudioRecorder>();

builder.Services.AddLogging();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EspSpectrum.Web.Client._Imports).Assembly);

app.Run();
