
namespace EspSpectrum.Core.Recording
{
    public interface IEspSpectrumRunner
    {
        void Start();
        ValueTask Tick(CancellationToken cancellationToken);
    }
}