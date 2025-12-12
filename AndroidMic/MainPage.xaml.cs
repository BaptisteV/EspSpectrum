using Android.Media;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using EspSpectrum.Core.Websocket;

namespace AndroidMic;

public partial class MainPage : ContentPage
{
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

    private readonly IEspSpectrumRunner _stableSpectrumReader; 
    
    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_stableSpectrumReader.WaitForNextTick(stoppingToken))
        {
            await _stableSpectrumReader.DoFftAndSend(stoppingToken);
            //await Task.Yield();
        }
    }
    public MainPage(IEspSpectrumRunner stableSpectrumReader)
    {
        InitializeComponent();
        _stableSpectrumReader = stableSpectrumReader;
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

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        await RequestMicrophonePermission();
        _stableSpectrumReader.Start();
         Task.Run(()=> ExecuteAsync(CancellationToken.None));
        //await _analyzer.Start();
        //_stableSpectrumReader.Start();
        //ExecuteTask.Start();
    }
}
