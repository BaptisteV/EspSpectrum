using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
    private readonly SpectrumBoxes spectrumBoxes;
    private readonly IEspSpectrumRunner _stableSpectrumReader;
    private readonly Task ExecuteTask;
    private readonly ILogger<MainPage> _logger;

    public MainPage(IEspSpectrumRunner stableSpectrumReader, ILogger<MainPage> logger)
    {
        _stableSpectrumReader = stableSpectrumReader;
        _logger = logger;
        InitializeComponent();
        spectrumBoxes = new SpectrumBoxes(SlidersStackLayout);
        ExecuteTask = new Task(async () => await ExecuteAsync(CancellationToken.None));
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var spectrum = await _stableSpectrumReader.Loop(stoppingToken);
                await MainThread.InvokeOnMainThreadAsync(() => spectrumBoxes.Update(spectrum.Bands));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in spectrum reading loop");
        }
    }

    private async Task RequestMicrophonePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission Denied", "Microphone permission is required", "OK");
        }
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await RequestMicrophonePermission();
        spectrumBoxes.Setup();
        await _stableSpectrumReader.Start();
        ExecuteTask.Start();
    }

    private async void SlidersStackLayout_SizeChanged(object sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(spectrumBoxes.OnSizeChanged);
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        spectrumBoxes.Clear();
    }
}
