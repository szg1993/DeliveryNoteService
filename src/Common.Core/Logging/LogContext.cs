using Microsoft.Extensions.Logging;

namespace Common.Core.Logging;

public static class LogContext
{
    public static IDisposable PushProperty(this ILogger logger, string propertyName, object propertyValue) => logger.BeginScope(
        new Dictionary<string, object>
        {
            [propertyName] = propertyValue
        })!;
}