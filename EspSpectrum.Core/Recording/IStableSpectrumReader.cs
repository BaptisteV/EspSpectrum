
namespace EspSpectrum.Core.Recording
{
    public interface IStableSpectrumReader
    {
        void Start();
        ValueTask Tick(CancellationToken cancellationToken);
    }
}