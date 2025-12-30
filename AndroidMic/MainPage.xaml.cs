using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;
using Microsoft.Extensions.Logging;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
    private readonly SpectrumGrid _spectrumGrid;
    private readonly IEspSpectrumRunner _runner;
    private readonly ISpectrumWebsocket _wsSpectrum;
    private readonly ILogger<MainPage> _logger;
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
                if (s is null)
                    return;
                VolumeLabel.Text = $"Volume: {s.Volume:F2}";
                _spectrumGrid.Update(s.Bands);
                UpdateConnectionBadge();
            });
        }));
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        _spectrumGrid.Setup();
        Task.Run(async () => await _runner.Start());
        UpdateConnectionBadge();
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

    private async void ContentPage_Unloaded(object sender, EventArgs e)
    {
        await _runner.Stop();
    }
}
