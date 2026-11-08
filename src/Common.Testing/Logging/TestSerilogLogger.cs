using Serilog.Events;

namespace Medusa.Enap.Common.Testing.Logging;

public class TestSerilogLogger(Action<LogEvent> eventHandler) : Serilog.ILogger
{
    public void Write(LogEvent logEvent) => eventHandler(logEvent);
}

