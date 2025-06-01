using EspSpectrum.Core.Display;
using EspSpectrum.Core.Fft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Threading.Channels;

namespace EspSpectrum.Core.Recording;

public sealed class FftRecorder : IFftRecorder, IDisposable
{
    private IWaveIn _waveIn;
    private readonly IOptionsMonitor<DisplayConfig> _optionsMonitor;
    private readonly ILogger<FftRecorder> _logger;

    private readonly Channel<Spectrum> _ffts;
    private readonly PeekableChannel<float> _peekableChannel;

    private readonly MMDeviceEnumerator _deviceEnumerator;

    // Required to properly work
#pragma warning disable S1450
    private readonly DeviceChangedNotifier _deviceChangedNotifier;
#pragma warning restore S1450

    private readonly FftProcessor _fftProcessor;

    public FftRecorder(ILogger<FftRecorder> logger, IWaveIn waveIn, IOptionsMonitor<DisplayConfig> optionsMonitor)
    {
        _logger = logger;
        _waveIn = waveIn;
        _optionsMonitor = optionsMonitor;

        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceChangedNotifier = new DeviceChangedNotifier(_logger, this);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_deviceChangedNotifier);

        _ffts = Channel.CreateBounded<Spectrum>(new BoundedChannelOptions(4)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        }, f =>
        {
            _logger.LogInformation("FFT dropped. If it happens too much (10+/sec), increase ReadLength");
        });

        var channel = Channel.CreateUnbounded<float>();

        _peekableChannel = new PeekableChannel<float>(channel);

        _fftProcessor = new FftProcessor(_waveIn.WaveFormat.SampleRate);
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
        await ProcessAudio(buffer, bytesRecorded);
    }

    private async ValueTask ProcessAudio(byte[] buffer, int bytesRecorded)
    {
        var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

        var channels = _waveIn.WaveFormat.Channels;

        for (var i = 0; i < bytesRecorded; i += bufferIncrement)
        {
            var channelsSum = 0f;
            for (var channel = 0; channel < channels; channel++)
            {
                var sampleOffset = i + channel * 4; // 4 bytes per float
                var sample = BitConverter.ToSingle(buffer, sampleOffset);

                channelsSum += sample;
            }
            await _peekableChannel.Writer.WriteAsync(channelsSum * (float)_optionsMonitor.CurrentValue.Amplification);

            while (_peekableChannel.Count() >= FftProps.FftLength)
            {
                await ProcesFFT();
            }
        }
    }

    private async ValueTask ProcesFFT()
    {
        var swRead = Stopwatch.StartNew();
        var audioData = (await _peekableChannel.ReadPartialConsume(FftProps.FftLength, FftProps.ReadLength)).ToArray();
        swRead.Stop();
        var swCompute = Stopwatch.StartNew();
        var fft = _fftProcessor.ToFft(audioData);
        swCompute.Stop();

        var swWrite = Stopwatch.StartNew();
        await _ffts.Writer.WriteAsync(fft);
        swWrite.Stop();
        _logger.LogInformation("New FFT written. Took {ElapsedRead}ms to read, {ElapsedCompute}ms to process, {ElapsedWrite}ms to write, {FFTCount} available",
             swRead.Elapsed.TotalMilliseconds, swCompute.Elapsed.TotalMilliseconds, swWrite.Elapsed.TotalMilliseconds, _ffts.Reader.Count);
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

    public async ValueTask<Spectrum> ReadFft(CancellationToken cancellationToken = default)
    {
        return await _ffts.Reader.ReadAsync(cancellationToken);
    }

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}
