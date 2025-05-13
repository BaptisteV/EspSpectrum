using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Threading.Channels;

namespace EspSpectrum.Core;

public class AudioRecorder : IAudioRecorder
{
    private readonly Channel<float> _data = Channel.CreateBounded<float>(FftProps.FftLength);
    private WasapiLoopbackCapture _waveIn;
    private readonly ILogger<AudioRecorder> _logger;

    public AudioRecorder(ILogger<AudioRecorder> logger)
    {
        _waveIn = new WasapiLoopbackCapture();
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _waveIn.StartRecording();
        _logger = logger;
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogError(e.Exception, "Recording stopped. Current state: {State}", _waveIn.CaptureState);
    }

    public int SampleRate => _waveIn.WaveFormat.SampleRate;

    public int ChannelCount => _waveIn.WaveFormat.Channels;

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var buffer = e.Buffer;
        var bytesRecorded = e.BytesRecorded;
        var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

        List<float> samples = [];
        for (var index = 0; index < bytesRecorded; index += bufferIncrement)
        {
            var sample32 = BitConverter.ToSingle(buffer, index);
            samples.Add(sample32);
        }

        foreach (var sample in samples)
        {
            _ = _data.Writer.TryWrite(sample);
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
