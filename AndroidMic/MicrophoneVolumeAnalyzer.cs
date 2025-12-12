using Android.Media;
using Android.Media.Metrics;
using EspSpectrum.Core.Fft;
using NAudio.MediaFoundation;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidMic;

public class MicrophoneVolumeAnalyzer : IDisposable
{
    private AudioRecord? _audioRecord;
    private bool _isRecording;
    private CancellationTokenSource? _cts;
    private const int SampleRate = 44100;
    private const ChannelIn ChannelConfig = ChannelIn.Mono;
    private const Encoding AudioFormat = Encoding.Pcm16bit;
    private const int FftSize = 4096; // Must be power of 2
    private const int BandCount = 32;
    private int _bufferSize;
    private readonly FftProcessor _fftProcessor = new(SampleRate);

    public event EventHandler<VolumeEventArgs>? VolumeChanged;

    public MicrophoneVolumeAnalyzer()
    {
        _bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);

        // Increase buffer size for better performance
        _bufferSize *= 2;
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

    public void Stop()
    {
        if (!_isRecording)
            return;

        _isRecording = false;
        _cts?.Cancel();

        _audioRecord?.Stop();
        _audioRecord?.Release();
        _audioRecord?.Dispose();
        _audioRecord = null;
    }

    private void RecordLoop(CancellationToken token)
    {
        var buffer = new short[_bufferSize / 2];
        var fftBuffer = new short[FftSize];
        var fftBufferPos = 0;

        while (_isRecording && !token.IsCancellationRequested)
        {
            try
            {
                var read = _audioRecord?.Read(buffer, 0, buffer.Length) ?? 0;

                if (read > 0)
                {
                    // Calculate volume metrics
                    var amplitude = CalculateAmplitude(buffer, read);
                    var db = AmplitudeToDecibels(amplitude);
                    var normalizedVolume = NormalizeVolume(db);

                    // Accumulate samples for FFT
                    for (int i = 0; i < read && fftBufferPos < FftSize; i++)
                    {
                        fftBuffer[fftBufferPos++] = buffer[i];
                    }

                    var fftBands2 = _fftProcessor.ToFft(fftBuffer.Select(sh => (float)sh).ToArray().AsSpan());

                    VolumeChanged?.Invoke(this, new VolumeEventArgs
                    {
                        Amplitude = amplitude,
                        Decibels = db,
                        NormalizedVolume = normalizedVolume,
                        FFTSpectrum = fftBands2,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading audio: {ex.Message}");
                break;
            }
        }
    }

    private double CalculateAmplitude(short[] buffer, int length)
    {
        long sum = 0;

        for (int i = 0; i < length; i++)
        {
            sum += Math.Abs(buffer[i]);
        }

        return (double)sum / length;
    }

    private double AmplitudeToDecibels(double amplitude)
    {
        if (amplitude <= 0)
            return -160; // Minimum practical dB value

        // Reference amplitude for 16-bit audio
        const double reference = 32768.0;
        return 20 * Math.Log10(amplitude / reference);
    }

    private double NormalizeVolume(double decibels)
    {
        // Normalize dB range (-60 to 0) to (0 to 1)
        const double minDb = -60;
        const double maxDb = 0;

        var normalized = (decibels - minDb) / (maxDb - minDb);
        return Math.Clamp(normalized, 0, 1);
    }

    private double[] ComputeFFTBands(short[] samples)
    {
        // Convert samples to complex numbers and apply Hamming window
        var complex = new Complex[FftSize];
        for (int i = 0; i < FftSize; i++)
        {
            // Hamming window
            var window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (FftSize - 1));
            complex[i] = new Complex(samples[i] * window / 32768.0, 0);
        }

        // Perform FFT
        FFT(complex);

        // Calculate frequency resolution
        var freqResolution = (double)SampleRate / FftSize;
        var maxFreq = 15000.0;
        var maxBin = (int)(maxFreq / freqResolution);

        // Logarithmic band distribution for better visualization
        var bands = new double[BandCount];
        var logMin = Math.Log10(20); // Start from 20 Hz
        var logMax = Math.Log10(maxFreq);
        var logStep = (logMax - logMin) / BandCount;

        for (int i = 0; i < BandCount; i++)
        {
            var freqLow = Math.Pow(10, logMin + i * logStep);
            var freqHigh = Math.Pow(10, logMin + (i + 1) * logStep);

            var binLow = (int)(freqLow / freqResolution);
            var binHigh = (int)(freqHigh / freqResolution);

            binLow = Math.Max(1, binLow);
            binHigh = Math.Min(maxBin, binHigh);

            // Average magnitude for this band
            double sum = 0;
            int count = 0;
            for (int j = binLow; j < binHigh && j < FftSize / 2; j++)
            {
                var magnitude = complex[j].Magnitude;
                sum += magnitude;
                count++;
            }

            if (count > 0)
            {
                var avgMagnitude = sum / count;
                // Normalize to 0-1 range with some scaling for visibility
                bands[i] = Math.Min(1.0, avgMagnitude * 10);
            }
        }

        return bands;
    }

    private void FFT(Complex[] data)
    {
        int n = data.Length;
        if (n <= 1) return;

        // Bit-reversal permutation
        int j = 0;
        for (int i = 0; i < n - 1; i++)
        {
            if (i < j)
            {
                var temp = data[i];
                data[i] = data[j];
                data[j] = temp;
            }

            int k = n / 2;
            while (k <= j)
            {
                j -= k;
                k /= 2;
            }
            j += k;
        }

        // Cooley-Tukey FFT
        for (int len = 2; len <= n; len *= 2)
        {
            double angle = -2 * Math.PI / len;
            var wlen = new Complex(Math.Cos(angle), Math.Sin(angle));

            for (int i = 0; i < n; i += len)
            {
                var w = Complex.One;
                for (int k = 0; k < len / 2; k++)
                {
                    var t = w * data[i + k + len / 2];
                    var u = data[i + k];
                    data[i + k] = u + t;
                    data[i + k + len / 2] = u - t;
                    w *= wlen;
                }
            }
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}

public class VolumeEventArgs : EventArgs
{
    public double Amplitude { get; init; }
    public double Decibels { get; init; }
    public double NormalizedVolume { get; init; }
    public Spectrum FFTSpectrum { get; init; } // 32 bands from 0-15kHz
}