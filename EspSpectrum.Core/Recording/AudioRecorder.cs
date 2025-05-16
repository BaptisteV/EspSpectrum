using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Threading.Channels;

namespace EspSpectrum.Core.Recording;

public class AudioRecorder : IAudioRecorder
{
    private readonly Channel<float> _data = Channel.CreateBounded<float>(
        new BoundedChannelOptions(FftProps.FftLength)
        {
            FullMode = BoundedChannelFullMode.DropNewest,
        });
    private IWaveIn _waveIn;
    private readonly ILogger<AudioRecorder> _logger;

    public AudioRecorder(ILogger<AudioRecorder> logger, IWaveIn waveIn)
    {
        _waveIn = waveIn;
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _waveIn.StartRecording();
        _logger = logger;
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogError(e.Exception, "Recording stopped");
    }

    public int SampleRate => _waveIn.WaveFormat.SampleRate;

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
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

            var written = _data.Writer.TryWrite(channelsSum);
            if (!written)
                _logger.LogWarning("Failed to write data");
        }
    }

    public async Task<float[]> ReadN(int length)
    {
        if (_data.Reader.Count < length)
        {
            _logger.LogDebug("Not enough recording trying to read {ReadLength}", length);
            return [];
        }

        var part = new List<float>(length);
        var reader = _data.Reader;

        while (part.Count < length && await reader.WaitToReadAsync())
        {
            if (reader.TryRead(out var item))
            {
                part.Add(item);
            }
            else
            {
                _logger.LogWarning("Failed to read data");
            }
        }

        return [.. part];
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
}
