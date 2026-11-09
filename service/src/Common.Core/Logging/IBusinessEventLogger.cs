using Common.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Common.Core.Logging;

public interface IBusinessEventLogger : IMedusaLogger
{
    void LogBusinessEvent(string? eventId = null, Dictionary<string, object?>? logParams = null, LogLevel? logLevel = null);

    void LogBusinessEventError(Exception ex);

    void LogBusinessEventError(string eventId, Exception ex);

    void LogBusinessEventError(string eventId, MedusaError err);

    void LogBusinessEventError(string eventId, Dictionary<string, object?>? logParams = null);

    void LogBusinessEventWarning(string eventId, Dictionary<string, object?>? logParams = null);

    void LogBusinessEventWarning(string eventId, Exception ex);
}