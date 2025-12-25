using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
    private readonly SpectrumBoxes _spectrumBoxes;
    private readonly IEspSpectrumRunner _stableSpectrumReader;
    private readonly ISpectrumWebsocket _wsSpectrum;
    private Task ExecuteTask;
    private readonly ILogger<MainPage> _logger;
    public MainPage(IEspSpectrumRunner stableSpectrumReader, ISpectrumWebsocket wsSpectrum, ILogger<MainPage> logger)
    {
        _stableSpectrumReader = stableSpectrumReader;
        _wsSpectrum = wsSpectrum;
        _logger = logger;
        InitializeComponent();
        _spectrumBoxes = new SpectrumBoxes(SlidersStackLayout);
        _spectrumBoxes.Setup();
        ExecuteTask = new Task(async () => await ExecuteAsync(CancellationToken.None), TaskCreationOptions.LongRunning);
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var spectrum = await _stableSpectrumReader.Loop(stoppingToken);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                VolumeLabel.Text = $"Volume: {spectrum.Volume:F2}";
                _spectrumBoxes.Update(spectrum.Bands);
                UpdateConnectionBadge();
            });
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
        await _stableSpectrumReader.Start();
        UpdateConnectionBadge();

        if (ExecuteTask.Status != TaskStatus.Running && ExecuteTask.Status != TaskStatus.RanToCompletion)
            ExecuteTask.Start();
    }

    private async void SlidersStackLayout_SizeChanged(object sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(_spectrumBoxes.OnSizeChanged);
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        await _wsSpectrum.TryConnect();
    }

    private void UpdateConnectionBadge()
    {
        var connected = _wsSpectrum.IsConnected();
        if (connected)
        {
            ConnectionStateBadge.Background = Colors.DarkGreen;
        }
        else
        {
            ConnectionStateBadge.Background = Colors.DarkRed;
        }
    }
}
