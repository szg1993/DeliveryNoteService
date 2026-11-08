using Medusa.Enap.Common.Core.Logging;
using Serilog.Events;

namespace Medusa.Enap.Common.AspNetCore.Logging;

public class LogEntry(LogEvent logEvent)
{
    public string? LogLevel { get; } = logEvent.Level.ToMedusaLogLevel();
    public string CreatedAt { get; } = logEvent.Timestamp.ToIso8601String();
    public string? Module { get; } = logEvent.GetValue(nameof(Module));
    public string? Category { get; } = logEvent.GetValue(nameof(Category)) ?? LogConstants.Technical;
    public string? EventId { get; } =
        logEvent.IsMedusaLog() ? logEvent.GetValue(nameof(EventId)) ?? LogConstants.Technical : LogConstants.Technical;
    public string? ErrorCode { get; } = logEvent.GetValue(nameof(ErrorCode));
    public IDictionary<string, string>? Params { get; } =
        logEvent.IsMedusaLog() ? logEvent.GetParams() : logEvent.ConvertPropertiesToParams();
    public Guid CorrelationId { get; } = Guid.TryParse(logEvent.GetValue(nameof(CorrelationId)), out var correlationId) ? correlationId : Guid.Empty;
}

public class BusinessLogEntry(LogEvent logEvent) : LogEntry(logEvent)
{
    public string? BusinessIdType { get; } = logEvent.GetValue(nameof(BusinessIdType));
    public string? BusinessId { get; } = logEvent.GetValue(nameof(BusinessId));
    public string? UserId { get; } = logEvent.GetValue(nameof(UserId));
    public string[] Labels { get; } = logEvent.GetLabels();
    public string? LogId { get; } = logEvent.GetValue(nameof(LogId));
}

public static class LogEventExtensions
{
    public static LogEntry ToLogEntry(this LogEvent logEvent)
        => logEvent.IsBusinessLog()
            ? new BusinessLogEntry(logEvent)
            : new LogEntry(logEvent);

    public static bool IsMedusaLog(this LogEvent logEvent)
         => logEvent.IsBusinessLog() || logEvent.GetValue(nameof(BusinessLogEntry.EventId)) == LogConstants.Technical;

    public static bool IsBusinessLog(this LogEvent logEvent)
        => logEvent.Properties.ContainsKey(nameof(BusinessLogEntry.BusinessId));

    public static string? GetValue(this LogEvent logEvent, string key)
        => logEvent.Properties.TryGetValue(key, out var value) ? value.ToString().Trim('\"') : null;

    public static string[] GetLabels(this LogEvent logEvent)
        => logEvent.Properties.TryGetValue("Labels", out var labels)
            ? (labels as SequenceValue)?.Elements.Select(e => e.ToString().Trim('\"')).ToArray() ?? []
            : [];

    public static IDictionary<string, string>? GetParams(this LogEvent logEvent)
    {
        return logEvent.Properties.TryGetValue("Params", out var @params)
                ? (@params as DictionaryValue)?.Elements.ToDictionary(r => r.Key.ToString().Trim('\"'), r => r.Value.ToString().Trim('\"'))
                    ?? null
                : null;
    }

    public static IDictionary<string, string>? ConvertPropertiesToParams(this LogEvent logEvent)
    {
        var p = logEvent.Properties.ToDictionary(e => e.Key.Trim('\"'), e => e.Value.ToString().Trim('\"'));
        p.Add("Message", logEvent.RenderMessage());
        p.Add("MessageTemplate", logEvent.MessageTemplate.Text);
        return p;
    }

    public static string ToMedusaLogLevel(this LogEventLevel eventLevel)
         => eventLevel switch
         {
             LogEventLevel.Fatal => LogLevels.Fatal,
             LogEventLevel.Error => LogLevels.Error,
             LogEventLevel.Warning => LogLevels.Warn,
             LogEventLevel.Information => LogLevels.Info,
             LogEventLevel.Debug => LogLevels.Debug,
             LogEventLevel.Verbose => LogLevels.Trace,
             _ => LogLevels.Warn
         };
}
