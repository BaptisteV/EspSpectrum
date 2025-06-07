using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace EspSpectrum.Core.Recording;

public sealed class FftRecorderSpan : IFftRecorder, IDisposable
{
    private IWaveIn _waveIn;
    private readonly IOptionsMonitor<DisplayConfig> _optionsMonitor;
    private readonly ILogger<FftRecorderSpan> _logger;

    private readonly MMDeviceEnumerator _deviceEnumerator;

    // Required to properly work
#pragma warning disable S1450
    private readonly DeviceChangedNotifier _deviceChangedNotifier;
#pragma warning restore S1450

    private readonly FftProcessor _fftProcessor;

    public FftRecorderSpan(ILogger<FftRecorderSpan> logger, IWaveIn waveIn, IOptionsMonitor<DisplayConfig> optionsMonitor)
    {
        _logger = logger;
        _waveIn = waveIn;
        _optionsMonitor = optionsMonitor;

        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceChangedNotifier = new DeviceChangedNotifier(_logger, this);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceChangedNotifier);
        _buffPartialReader = new PartialDataReader(logger, FftProps.FftLength, FftProps.ReadLength);

        _fftProcessor = new FftProcessor(_waveIn.WaveFormat.SampleRate);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogError(e.Exception, "Recording stopped");
    }

    private readonly PartialDataReader _buffPartialReader;

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        ReadOnlySpan<float> newData = ReadAudioSpan(e.Buffer, e.BytesRecorded);
        _buffPartialReader.AddData(newData);
    }

    private ReadOnlySpan<float> ReadAudioSpan(ReadOnlySpan<byte> buffer, int bytesRecorded)
    {
        var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

        var channels = _waveIn.WaveFormat.Channels;

        var sampleCount = bytesRecorded / bufferIncrement;
        var a = new float[sampleCount];
        var ia = 0;

        for (var i = 0; i < bytesRecorded; i += bufferIncrement)
        {
            var channelsSum = 0f;
            for (var channel = 0; channel < channels; channel++)
            {
                var sampleOffset = i + channel * 4; // 4 bytes per float
                MemoryMarshal.TryRead<float>(buffer.Slice(sampleOffset, 4), out var sample);
                channelsSum += sample;
            }

            a[ia] = channelsSum * (float)_optionsMonitor.CurrentValue.Amplification;
            ia++;
        }

        return a;
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
        // TODO Use service provider
        _waveIn = new WasapiLoopbackCapture();
        Start();
    }

    public Spectrum? TryReadFft(CancellationToken cancellationToken = default)
    {
        var didRead = _buffPartialReader.TryRead(out var read);
        if (!didRead)
        {
            _logger.LogDebug("Unable to read from buffer, not enough data ?");
            return null;
        }

        var spectrum = _fftProcessor.ToFft(read);
        return spectrum;
    }

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}
