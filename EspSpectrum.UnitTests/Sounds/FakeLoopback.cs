using NAudio.Wave;

namespace EspSpectrum.UnitTests.Sounds;

public sealed class FakeLoopbackWaveIn : IWaveIn
{
    public WaveFormat WaveFormat { get; set; } = new WaveFormat(44100, 32, 2);

    public event EventHandler<WaveInEventArgs>? DataAvailable;
    public event EventHandler<StoppedEventArgs>? RecordingStopped;
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Dispose();
    }

    public void RecordSingleSine()
    {
        var s = Sine440.Buffer;
        var sine = new List<byte>();
        for (var iBuff = 0; iBuff < s.Length; iBuff++)
        {
            var chunck = BitConverter.GetBytes((float)s[iBuff]);
            for (var c = 0; c < WaveFormat.Channels; c++)
            {
                sine.AddRange(chunck);
            }
        }
        DataAvailable?.Invoke(this, new WaveInEventArgs([.. sine], sine.Count));
    }

    public void StartRecording()
    {
    }

    public void StopRecording()
    {
        _cts.Cancel();
        RecordingStopped?.Invoke(this, new StoppedEventArgs());
    }
}
