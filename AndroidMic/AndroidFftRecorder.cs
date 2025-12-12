using Android.Media;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AndroidMic;

public class AndroidFftRecorder : IFftRecorder
{
    private AudioRecord? _audioRecord;
    private bool _isRecording;
    private CancellationTokenSource? _cts;
    private const int SampleRate = 44100;
    private const ChannelIn ChannelConfig = ChannelIn.Mono;
    private const Android.Media.Encoding AudioFormat = Android.Media.Encoding.Pcm16bit;
    private const int FftSize = 4096; // Must be power of 2
    private const int BandCount = 32;
    private int _bufferSize;
    private readonly FftProcessor _fftProcessor = new(SampleRate);
    private readonly IDataReader _buffReader; 
    private readonly IOptionsMonitor<DisplayConfig> _optionsMonitor;

    private readonly ILogger<AndroidFftRecorder> _logger;
    public AndroidFftRecorder(IDataReader dr, IOptionsMonitor<DisplayConfig> optionsMonitor, ILogger<AndroidFftRecorder> logger)
    {
        _buffReader = dr;
        _optionsMonitor = optionsMonitor;
        _logger = logger;

        _bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);

        // Increase buffer size for better performance
        _bufferSize *= 2;
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    public void Restart()
    {
        throw new NotImplementedException();
    }

    public async Task Start()
    {
        if (_isRecording)
            return;

        _audioRecord = new AudioRecord(
            AudioSource.Mic,
            SampleRate,
            ChannelConfig,
            AudioFormat,
            _bufferSize);

        if (_audioRecord.State != State.Initialized)
        {
            throw new Exception("Failed to initialize AudioRecord");
        }

        _isRecording = true;
        _cts = new CancellationTokenSource();

        _audioRecord.StartRecording();

        await Task.Run(() => RecordLoop(_cts.Token));
    }

    private ReadOnlySpan<float> ReadAudioSpan(ReadOnlySpan<short> buffer, int samplesRead)
    {
        // Get amplification from config
        var amplification = (float)_optionsMonitor.CurrentValue.Amplification;

        // Allocate float array for converted samples
        var samplesSpan = new float[samplesRead];

        // Convert short samples to float and apply amplification
        // Android AudioRecord with Pcm16bit and Mono gives us direct short samples
        // Range: -32768 to 32767 -> normalize to -1.0 to 1.0
        for (var i = 0; i < samplesRead; i++)
        {
            // Normalize int16 to float range [-1.0, 1.0] and apply amplification
            samplesSpan[i] = (buffer[i] / 32768.0f) * amplification;
        }

        return samplesSpan;
    }

    private void RecordLoop(CancellationToken token)
    {
        var buffer = new short[_bufferSize / 2];

        while (_isRecording && !token.IsCancellationRequested)
        {
            try
            {
                var read = _audioRecord?.Read(buffer, 0, buffer.Length) ?? 0;

                if (read > 0)
                {
                    ReadOnlySpan<float> newData = ReadAudioSpan(buffer, read);
                    _buffReader.AddData(newData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading audio: {ex.Message}");
                break;
            }
        }
    }

    public bool TryReadSpectrum(out Spectrum spectrum, CancellationToken cancellationToken)
    {
        if (_buffReader.Count() < FftProps.FftLength)
        {
            _logger.LogTrace("Not enough data for a new Spectrum");
            spectrum = null;
            return false;
        }

        Span<float> buffer = stackalloc float[FftProps.FftLength];

        if(_buffReader.TryReadAudioFrame(buffer))
        {
            _logger.LogDebug("Got new audio frame, computing FFT");
            spectrum = _fftProcessor.ToFft(buffer);
            return true;
        }
        else
        {
            _logger.LogDebug("No spectrum available");
            spectrum = null;
            return false;
        }
    }
}
