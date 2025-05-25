using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Threading.Channels;

namespace EspSpectrum.Core.Recording;

public sealed class FftRecorder : IFftRecorder, IDisposable
{
    private IWaveIn _waveIn;
    private readonly ILogger<FftRecorder> _logger;

    private readonly Channel<Spectrum> _ffts;
    private readonly PeekableChannel<float> _peekableChannel;

    private readonly MMDeviceEnumerator _deviceEnumerator;

    // Required to properly work
#pragma warning disable S1450
    private readonly DeviceChangedNotifier _deviceChangedNotifier;
#pragma warning restore S1450

    public FftRecorder(ILogger<FftRecorder> logger, IWaveIn waveIn)
    {
        _logger = logger;
        _waveIn = waveIn;
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceChangedNotifier = new DeviceChangedNotifier(_logger, this);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceChangedNotifier);
        _waveIn.StartRecording();
        _ffts = Channel.CreateBounded<Spectrum>(new BoundedChannelOptions(8)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        }, f =>
        {
            _logger.LogInformation("FFT dropped. If it happens too much (10+/sec), increase ReadLength");
        });

        var channel = Channel.CreateBounded<float>(new BoundedChannelOptions(FftProps.FftLength * 2)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        }, d =>
        {
            _logger.LogWarning("Dropped sample");
        });

        _peekableChannel = new PeekableChannel<float>(channel);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogError(e.Exception, "Recording stopped");
    }

    public int SampleRate => _waveIn.WaveFormat.SampleRate;

    private async void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var buffer = e.Buffer;
        var bytesRecorded = e.BytesRecorded;
        var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

        var channels = _waveIn.WaveFormat.Channels;

        for (var i = 0; i < bytesRecorded; i += bufferIncrement)
        {
            var channelsSum = 0f;
            for (var channel = 0; channel < channels; channel++)
            {
                var sampleOffset = i + channel * 4; // 4 bytes per float
                var sample = BitConverter.ToSingle(buffer, sampleOffset) * (float)FftProps.Amplification;

                channelsSum += sample;
            }
            var written = _peekableChannel.Writer.TryWrite(channelsSum);
            if (!written)
                _logger.LogWarning("Failed to write data");

            if (_peekableChannel.Count >= FftProps.FftLength)
            {
                await ProcesFFT();
            }
        }
    }

    private async Task ProcesFFT()
    {
        var data = (await _peekableChannel.ReadPartialConsume(FftProps.FftLength, FftProps.ReadLength)).ToArray();
        var fft = FftProcessor.ToFft(data, _waveIn.WaveFormat.SampleRate);
        await _ffts.Writer.WriteAsync(fft);
    }

    public void Restart()
    {
        _waveIn.StopRecording();
        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.RecordingStopped -= OnRecordingStopped;
        _waveIn = new WasapiLoopbackCapture();
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;
        _waveIn.StartRecording();
    }

    public async Task<Spectrum> ReadFft(CancellationToken cancellationToken = default)
    {
        var fft = await _ffts.Reader.ReadAsync(cancellationToken);
        return fft;
    }

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}
