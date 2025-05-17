using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Threading.Channels;

namespace EspSpectrum.Core.Recording;

public class FftRecorder : IFftRecorder
{
    private IWaveIn _waveIn;
    private readonly ILogger<FftRecorder> _logger;

    private readonly Channel<FftResult> _ffts;
    private readonly PeekableChannel<float> _peekableChannel;

    public FftRecorder(ILogger<FftRecorder> logger, IWaveIn waveIn)
    {
        _waveIn = waveIn;
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _waveIn.StartRecording();
        _logger = logger;
        _ffts = Channel.CreateBounded<FftResult>(new BoundedChannelOptions(8)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        }, f =>
        {
            _logger.LogInformation("FFT dropped");
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
        _logger.LogDebug("Stopping recording");
        _waveIn.StopRecording();
        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.RecordingStopped -= OnRecordingStopped;
        _waveIn = new WasapiLoopbackCapture();
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;
        _waveIn.StartRecording();
        _logger.LogDebug("Started recording");
    }

    public async Task<FftResult> ReadFft(CancellationToken cancellationToken = default)
    {
        var fft = await _ffts.Reader.ReadAsync(cancellationToken);
        return fft;
    }
}
