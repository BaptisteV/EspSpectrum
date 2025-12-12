using Android.Media;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
    private readonly MicrophoneVolumeAnalyzer _analyzer;
    private readonly ProgressBar[] _progressBars;

    private string[] To8Bands(double[] bands32)
    {
        var result = new double[8];
        var iR = 0;
        for (int i = 0; i < 32; i+=4)
        {
            // Average every 4 bands from the 32-band array
            result[iR] = bands32[i];
            iR++;
        }
        return result.Select(f => f.ToString("N2")).ToArray();
    }

    private readonly ISpectrumWebsocket _ws;
    //private readonly IEspSpectrumRunner _stableSpectrumReader;
    public MainPage(ISpectrumWebsocket ws/*, IEspSpectrumRunner stableSpectrumReader*/)
    {
        InitializeComponent();
        _ws = ws;
        //_stableSpectrumReader = stableSpectrumReader;

        _progressBars =
        [
            //FrequencyProgressBar1,
            //FrequencyProgressBar2,
            //FrequencyProgressBar3,
            //FrequencyProgressBar4,
            //FrequencyProgressBar5,
            //FrequencyProgressBar6,
            //FrequencyProgressBar7,
            //FrequencyProgressBar8,
        ];

        _analyzer = new MicrophoneVolumeAnalyzer();

        _analyzer.VolumeChanged += async (sender, e) =>
        {
            Console.WriteLine($"FFT {string.Join("\t", To8Bands(e.FFTSpectrum.Bands))}");
            //UpdateFrequencyBars(e.FFTSpectrum.Bands);
            await _ws.SendSpectrum(e.FFTSpectrum);
            Console.WriteLine();

            await MainThread.InvokeOnMainThreadAsync(() => {
                VolumeLabel.Text = $"Volume: {e.NormalizedVolume:P0}";
                VolumeProgressBar.Progress = e.NormalizedVolume;
            });
        };
    }

    private void UpdateFrequencyBars(double[] bands)
    {
        for (int i = 0; i < Math.Min(bands.Length, _progressBars.Length); i++)
        {
            var normalizedValue = Math.Clamp(bands[i] / 200.0, 0.0, 1.0);
            _progressBars[i].Progress = normalizedValue;
        }
    }

    private async Task RequestMicrophonePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Microphone permission is required", "OK");
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        await _analyzer.Start();
    }

    private void StopRecording()
    {
        _analyzer.Stop();
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await RequestMicrophonePermission();
        await _analyzer.Start();
    }
}
