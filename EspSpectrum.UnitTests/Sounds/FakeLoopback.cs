using NAudio.Wave;

namespace EspSpectrum.UnitTests.Sounds;

public sealed class FakeLoopbackWaveIn : IWaveIn
{
    public WaveFormat WaveFormat { get; set; } = new WaveFormat();

    public event EventHandler<WaveInEventArgs> DataAvailable;
    public event EventHandler<StoppedEventArgs> RecordingStopped;
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Dispose();
    }

    public void FakeRecord(byte[] data, int bytesRecorded)
    {
        DataAvailable.Invoke(this, new WaveInEventArgs(data, bytesRecorded));
    }

    public void StartRecording()
    {
    }

    public void StopRecording()
    {
        _cts.Cancel();
        RecordingStopped.Invoke(this, new StoppedEventArgs());
    }
}
