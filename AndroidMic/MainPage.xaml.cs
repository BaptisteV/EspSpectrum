using Android.Media;

namespace AndroidMic;


public partial class MainPage : ContentPage
{
    private AudioRecord audioRecord;
    private bool isRecording = false;
    private CancellationTokenSource cancellationTokenSource;

    // Audio configuration
    private const int SampleRate = 44100;
    private const ChannelIn ChannelConfig = ChannelIn.Mono;
    private const Encoding AudioFormat = Encoding.Pcm16bit;
    private int bufferSize;

    public MainPage()
    {
        InitializeComponent();

        // Calculate buffer size
        bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);

        // Request microphone permission
        RequestMicrophonePermission();
    }

    private async void RequestMicrophonePermission()
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Microphone permission is required", "OK");
        }
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        if (!isRecording)
        {
            StartRecording();
            StartButton.Text = "Stop Recording";
            StatusLabel.Text = "Recording...";
        }
        else
        {
            StopRecording();
            StartButton.Text = "Start Recording";
            StatusLabel.Text = "Stopped";
        }
    }

    private void StartRecording()
    {
        try
        {
            audioRecord = new AudioRecord(
                AudioSource.Mic,
                SampleRate,
                ChannelConfig,
                AudioFormat,
                bufferSize);

            if (audioRecord.State != State.Initialized)
            {
                StatusLabel.Text = "Error: AudioRecord not initialized";
                return;
            }

            audioRecord.StartRecording();
            isRecording = true;

            Task.Run(() => ReadAudioData(CancellationToken.None));
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            });
        }
    }

    private void ReadAudioData(CancellationToken token)
    {
        short[] audioBuffer = new short[bufferSize / 2];

        while (isRecording && !token.IsCancellationRequested)
        {
            int readResult = audioRecord.Read(audioBuffer, 0, audioBuffer.Length);

            if (readResult > 0)
            {
                // Analyze audio data here
                AnalyzeAudioData(audioBuffer, readResult);
            }
        }
    }

    private void AnalyzeAudioData(short[] buffer, int length)
    {
        // Calculate average amplitude (volume level)
        long sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += Math.Abs(buffer[i]);
        }
        double average = (double)sum / length;

        // Calculate amplitude as percentage (0-100)
        double amplitudePercent = (average / short.MaxValue) * 100;

        // Update UI on main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            VolumeLabel.Text = $"Volume: {amplitudePercent:F2}%";
            VolumeProgressBar.Progress = amplitudePercent / 100.0;
        });
    }

    private void StopRecording()
    {
        isRecording = false;
        cancellationTokenSource?.Cancel();

        if (audioRecord != null)
        {
            if (audioRecord.RecordingState == RecordState.Recording)
            {
                audioRecord.Stop();
            }
            audioRecord.Release();
            audioRecord.Dispose();
            audioRecord = null;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            VolumeLabel.Text = "Volume: 0%";
            VolumeProgressBar.Progress = 0;
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (isRecording)
        {
            StopRecording();
        }
    }
}
