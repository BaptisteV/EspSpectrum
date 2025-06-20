using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests;

public class BaseTests(ITestOutputHelper testOutputHelper)
{
    protected readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XUnitLoggerProvider(testOutputHelper));
        });
}
