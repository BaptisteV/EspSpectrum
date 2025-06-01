using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public class BaseTests
{
    protected readonly ILoggerFactory LoggerFactory;
    public BaseTests(ITestOutputHelper testOutputHelper)
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
        });
    }
}
