using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace EspSpectrum.Core.Recording;

public sealed class FftRecorder : IFftRecorder
{
    private IWaveIn _waveIn;
    private readonly IOptionsMonitor<DisplayConfig> _optionsMonitor;
    private readonly ILogger<FftRecorder> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MMDeviceEnumerator _deviceEnumerator;

    // Required to properly work
#pragma warning disable S1450
    private readonly DeviceChangedNotifier _deviceChangedNotifier;
#pragma warning restore S1450

    private readonly FftProcessor _fftProcessor;
    private readonly IDataReader _buffReader;

    public FftRecorder(
        ILogger<FftRecorder> logger,
        IServiceProvider serviceProvider,
        IWaveIn waveIn,
        IOptionsMonitor<DisplayConfig> optionsMonitor,
        IDataReader dataReader)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _waveIn = waveIn;
        _optionsMonitor = optionsMonitor;

        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceChangedNotifier = new DeviceChangedNotifier(_logger, this);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceChangedNotifier);
        _buffReader = dataReader;

        _fftProcessor = new FftProcessor(_waveIn.WaveFormat.SampleRate);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogError(e.Exception, "Recording stopped");
    }


    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        ReadOnlySpan<float> newData = ReadAudioSpan(e.Buffer, e.BytesRecorded);
        _buffReader.AddData(newData);
    }

    private ReadOnlySpan<float> ReadAudioSpan(ReadOnlySpan<byte> buffer, int bytesRecorded)
    {
        var bufferIncrement = _waveIn.WaveFormat.BlockAlign;
        var channels = _waveIn.WaveFormat.Channels;
        var sampleCount = bytesRecorded / bufferIncrement;
        var amplification = (float)_optionsMonitor.CurrentValue.Amplification;

        var samplesSpan = new float[sampleCount];

        var floatBuffer = MemoryMarshal.Cast<byte, float>(buffer);

        var sampleIndex = 0;

        for (var i = 0; i < bytesRecorded; i += bufferIncrement)
        {
            var channelsSum = 0f;
            var floatOffset = i / 4; // Convert byte offset to float offset

            for (var channel = 0; channel < channels; channel++)
            {
                channelsSum += floatBuffer[floatOffset + channel];
            }


            samplesSpan[sampleIndex] = channelsSum * amplification;
            sampleIndex++;
        }

        return samplesSpan;
    }

    public void Start()
    {
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;
        _waveIn.StartRecording();
    }

    public void Restart()
    {
        _waveIn.StopRecording();
        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.RecordingStopped -= OnRecordingStopped;

        _waveIn = _serviceProvider.GetRequiredService<IWaveIn>();
        Start();
    }

    public bool TryReadSpectrum(out Spectrum? spectrum, CancellationToken cancellationToken)
    {
        if (_buffReader.Count() < FftProps.FftLength)
        {
            _logger.LogTrace("Not enough data for a new Spectrum");
            spectrum = default;
            return false;
        }

        Span<float> buffer = stackalloc float[FftProps.FftLength];
        var didRead = _buffReader.TryReadAudioFrame(buffer);
        if (!didRead)
        {
            _logger.LogDebug("No spectrum available");
            spectrum = default;
            return false;
        }

        spectrum = _fftProcessor.ToFft(buffer);
        return didRead;

    }

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}
