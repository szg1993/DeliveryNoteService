using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;

namespace Medusa.Enap.Common.AspNetCore.Logging;

public class MedusaLogFormatter : ITextFormatter
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
