using Android.Media;
using Android.Util;
using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using EspSpectrum.Core.Recording;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AndroidMic.Platforms;

public sealed class PlatformFftRecorder : IFftRecorder
{
    private AudioRecord? _audioRecord;
    private bool _isRecording;
    private readonly CancellationTokenSource _cts;
    private const int SampleRate = 44100;
    private const ChannelIn ChannelConfig = ChannelIn.Mono;
    private const Encoding AudioFormat = Encoding.Pcm16bit;
    private readonly int _bufferSize;
    private readonly FftProcessor _fftProcessor = new(SampleRate);
    private readonly IDataReader _buffReader;
    private readonly IOptionsMonitor<DisplayConfig> _optionsMonitor;

    private readonly ILogger<PlatformFftRecorder> _logger;

    // To avoid allocating too much
    private readonly float[] _floatBuffer;

    public PlatformFftRecorder(IDataReader dr, IOptionsMonitor<DisplayConfig> optionsMonitor, ILogger<PlatformFftRecorder> logger)
    {
        _buffReader = dr;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _cts = new CancellationTokenSource();

        _bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);
        _bufferSize *= 2;
        _floatBuffer = new float[_bufferSize / 2];
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    public void Restart()
    {
        _isRecording = false;
        _audioRecord!.Stop();
        _audioRecord.Dispose();
        Start();
    }
    private Thread? _recordThread;
    public void Start()
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
            throw new AndroidException("Failed to initialize AudioRecord");
        }

        _isRecording = true;

        _audioRecord.StartRecording();

        _recordThread = new Thread(() => RecordLoop(_cts.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.Highest
        };
        _recordThread.Start();
        //Task.Run(() => RecordLoop(_cts.Token));
    }

    private ReadOnlySpan<float> ReadAudioSpan(ReadOnlySpan<short> buffer, int samplesRead)
    {
        var amplification = (float)_optionsMonitor.CurrentValue.Amplification;
        var target = _floatBuffer.AsSpan(0, samplesRead);

        for (int i = 0; i < samplesRead; i++)
            target[i] = (buffer[i] / 32768f) * amplification;

        return target;
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
                    var newData = ReadAudioSpan(buffer, read);
                    _buffReader.AddData(newData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading audio");
                break;
            }
        }
    }

    public bool TryReadSpectrum(out Spectrum? spectrum, CancellationToken cancellationToken)
    {
        spectrum = default;
        if (_buffReader.Count() < FftProps.FftLength)
        {
            _logger.LogTrace("Not enough data for a new spectrum");
            return false;
        }

        Span<float> buffer = stackalloc float[FftProps.FftLength];

        if (_buffReader.TryReadAudioFrame(buffer))
        {
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
