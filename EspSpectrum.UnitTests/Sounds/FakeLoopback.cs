using NAudio.Wave;

namespace EspSpectrum.UnitTests.Sounds;

internal class FakeLoopbackWaveIn : IWaveIn
{
    private WaveFormat wf = new WaveFormat();
    public WaveFormat WaveFormat { get => wf; set => wf = value; }

    public event EventHandler<WaveInEventArgs> DataAvailable;
    public event EventHandler<StoppedEventArgs> RecordingStopped;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

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
