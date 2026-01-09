using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;
using static EspSpectrum.Core.Recording.IEspSpectrumRunner;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
    private readonly SpectrumGrid _spectrumGrid;
    private readonly IEspSpectrumRunner _runner;
    private readonly ISpectrumWebsocket _wsSpectrum;
    private readonly ILogger<MainPage> _logger;

    private readonly CancellationTokenSource _cts = new();

    public MainPage(IEspSpectrumRunner runner, ISpectrumWebsocket wsSpectrum, ILogger<MainPage> logger)
    {
        _runner = runner;
        _wsSpectrum = wsSpectrum;
        _logger = logger;
        InitializeComponent();
        _spectrumGrid = new SpectrumGrid(GridContainer);

        _runner.Subscribe(new SpectrumObserver(_logger, async (s) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                VolumeLabel.Text = $"Volume: {s.Volume:F2}";
                _spectrumGrid.Update(s.Bands);
                UpdateConnectionBadge();
            });
        }));
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        _spectrumGrid.Setup();
        await RequestMicrophonePermission();
        await _runner.StartAudio(_cts.Token);

        _ = Task.Factory.StartNew(async () =>
        {
            await _runner.TryConnectEsp(_cts.Token);
            while (!_cts.IsCancellationRequested)
            {
                var loopResult = await _runner.Loop(_cts.Token);
                if (!loopResult.HasFlag(RunnerState.ConnectedToEsp))
                {
                    await _runner.TryConnectEsp(_cts.Token);
                }

            }
        }, TaskCreationOptions.LongRunning).ConfigureAwait(false);

        UpdateConnectionBadge();
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        await _wsSpectrum.TryConnect(_cts.Token);
        await MainThread.InvokeOnMainThreadAsync(UpdateConnectionBadge);
    }

    private async Task RequestMicrophonePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission Denied", "Microphone permission is required", "OK");
        }
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

    private async void ContentPage_Unloaded(object sender, EventArgs e)
    {
        await _runner.Stop();
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
