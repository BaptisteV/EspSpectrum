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
    private readonly short[] _shortBuffer;

    public PlatformFftRecorder(IDataReader dr, IOptionsMonitor<DisplayConfig> optionsMonitor, ILogger<PlatformFftRecorder> logger)
    {
        _buffReader = dr;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _cts = new CancellationTokenSource();

        _bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);
        _bufferSize *= 2;
        _floatBuffer = new float[_bufferSize / 2];
        _shortBuffer = new short[_bufferSize / 2];
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _audioRecord?.Dispose();
    }

    public void Restart()
    {
        _isRecording = false;
        _audioRecord!.Stop();
        _audioRecord.Dispose();
        Start();
    }

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

        _ = Task.Factory.StartNew(() =>
        {
            RecordLoop(CancellationToken.None);
            // Long-running CPU work
        }, TaskCreationOptions.LongRunning).ConfigureAwait(false);

        //_ = Task.Run(() => RecordLoop(CancellationToken.None)).ConfigureAwait(false);
    }

    private ReadOnlySpan<float> ReadAudioSpan(ReadOnlySpan<short> buffer, int samplesRead)
    {
        var amplification = (float)_optionsMonitor.CurrentValue.Amplification;
        var target = _floatBuffer.AsSpan(0, samplesRead);

        const float additionalGain = 8.0f; // Should not be needed, but added to vaguely match results from Windows platform
        for (int i = 0; i < samplesRead; i++)
            target[i] = (buffer[i] / 32768f) * amplification * additionalGain;

        return target;
    }

    private void RecordLoop(CancellationToken token)
    {
        while (_isRecording && !token.IsCancellationRequested)
        {
            try
            {
                var read = _audioRecord?.Read(_shortBuffer, 0, _shortBuffer.Length) ?? 0;

                if (read > 0)
                {
                    var newData = ReadAudioSpan(_shortBuffer, read);
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
}
