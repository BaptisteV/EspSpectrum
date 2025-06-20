using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EspSpectrum.UnitTests.Utils;

public sealed class XUnitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    private readonly ITestOutputHelper _output = output;

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_output, categoryName);
    }

    public void Dispose() { }
}

public class XUnitLogger(ITestOutputHelper output, string categoryName) : ILogger
{
    private readonly ITestOutputHelper _output = output;
    private readonly string _categoryName = categoryName;

    IDisposable ILogger.BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _output.WriteLine($"[{logLevel}] {_categoryName} - {formatter(state, exception)}");
        if (exception != null)
        {
            _output.WriteLine(exception.ToString());
        }
    }
}
