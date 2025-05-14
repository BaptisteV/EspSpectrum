using NAudio.Wave;

namespace EspSpectrum.UnitTests.Sounds;

public sealed class WavePlayer : IDisposable
{
    private readonly IWavePlayer waveOut;
    private readonly SineWaveProvider32 sineWave;

    public WavePlayer(float frequency = 440.0f, float amplitude = 0.3f, int sampleRate = 44100)
    {
        sineWave = new SineWaveProvider32(frequency, amplitude);

        waveOut = new WaveOutEvent();
        waveOut.Init(sineWave);
    }

    public void Play()
    {
        waveOut.Play();
    }

    public void Stop()
    {
        waveOut.Stop();
    }

    public void Dispose()
    {
        waveOut.Dispose();
    }
}
