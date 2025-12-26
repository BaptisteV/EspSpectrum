using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;

namespace AndroidMic.Platforms;

public class PlatformFftRecorder : FftRecorder
{
    public PlatformFftRecorder(ILogger<PlatformFftRecorder> logger, IServiceProvider serviceProvider, IWaveIn waveIn, IOptionsMonitor<DisplayConfig> optionsMonitor, IDataReader dataReader) : base(logger, serviceProvider, waveIn, optionsMonitor, dataReader)
    {
    }
}
