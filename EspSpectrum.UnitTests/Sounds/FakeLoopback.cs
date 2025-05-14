using NAudio.Wave;

namespace EspSpectrum.UnitTests.Sounds;

internal class FakeLoopbackWaveIn : IWaveIn
{
    private WaveFormat wf = new WaveFormat();
    public WaveFormat WaveFormat { get => wf; set => wf = value; }

    public event EventHandler<WaveInEventArgs> DataAvailable;
    public event EventHandler<StoppedEventArgs> RecordingStopped;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly PeriodicTimer Timer = new(TimeSpan.FromMilliseconds(2));

    public void Dispose()
    {
        _cts.Dispose();
    }

    public void FakeRecord(byte[] data)
    {
        DataAvailable.Invoke(this, new WaveInEventArgs(data, data.Length));
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
