using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

namespace Common.AspNetCore.Logging;

public class LogFormatter : ITextFormatter
{
    private static JsonSerializerOptions? _jsonLogOpts;

    public static JsonSerializerOptions StructuredLogOptions
        => _jsonLogOpts ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var entry = logEvent.ToLogEntry();
        var entryAsJson = JsonSerializer.Serialize(entry, entry.GetType(), StructuredLogOptions);
        output.WriteLine(entryAsJson);
    }
}