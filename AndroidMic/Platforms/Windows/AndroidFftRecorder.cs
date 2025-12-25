using EspSpectrum.Core.Display;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;

namespace AndroidMic.Platforms;

public class AndroidFftRecorder : FftRecorder
{
    public AndroidFftRecorder(ILogger<AndroidFftRecorder> logger, IServiceProvider serviceProvider, IWaveIn waveIn, IOptionsMonitor<DisplayConfig> optionsMonitor, IDataReader dataReader) : base(logger, serviceProvider, waveIn, optionsMonitor, dataReader)
    {
    }
}
