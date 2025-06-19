using BenchmarkDotNet.Attributes;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Channels;

namespace EspSpectrum.PerformanceTests;


[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
public class PartialDataReaderTests
{
    [Benchmark]
    public async Task ChannelTest()
    {
        var dataChannel = Channel.CreateUnbounded<float>();
        await FeederThread.FeedData(dataChannel, FftProps.FftLength);
        var dr = new PartialDataReader(NullLogger.Instance, FftProps.FftLength, FftProps.ReadLength);
        dr.AddData(Sine440.Buffer);
        dr.TryRead(out _);
    }
}
