using System.Threading.Channels;

namespace EspSpectrum.PerformanceTests;

public class FeederThread
{
    private readonly Thread _thread;
    private readonly Channel<float> _dataChannel;
    private readonly int _count;

    public FeederThread(Channel<float> dataChannel, TimeSpan interval, int count)
    {
        _dataChannel = dataChannel;
        _count = count;
        _thread = new Thread(async (o) =>
        {
            var pt = new PeriodicTimer(interval);
            while (await pt.WaitForNextTickAsync())
            {
                await FeedData(_dataChannel, _count);
            }
        });
    }

    public static async Task FeedData(Channel<float> dataChannel, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await dataChannel.Writer.WriteAsync(i);
        }
    }

    public async Task Start(bool init)
    {
        _thread.Start();
        if (init) { await FeedData(_dataChannel, _count); }
    }

    public void Stop()
    {
        _thread.Join();
        _thread.Interrupt();
    }
}
