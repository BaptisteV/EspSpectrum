using EspSpectrum.Core.Recording;
using EspSpectrum.UnitTests.Sounds;
using Microsoft.Extensions.Logging.Abstractions;

namespace EspSpectrum.UnitTests;

public sealed class RecorderTests : IDisposable
{
    //private readonly WavePlayer _player = new();
    private readonly FakeLoopbackWaveIn _fakeLoopbackWaveIn = new();
    private readonly FftRecorder recorder;

    public RecorderTests()
    {
        recorder = new FftRecorder(NullLogger<FftRecorder>.Instance, _fakeLoopbackWaveIn);

        //_player.Play();
    }

    [Fact]
    public async Task Test1()
    {
        _fakeLoopbackWaveIn.FakeRecord([1, 2, 3, 4]);
        //var unDeux = await recorder.ReadN(1);
        //Assert.NotNull(unDeux);
        //var rien = await recorder.ReadN(1);
        //Assert.Empty(rien);
    }

    public void Dispose()
    {
        //_player.Stop();
    }
}
