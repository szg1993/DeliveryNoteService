using Medusa.Enap.Common.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.Core.Logging;

public interface IMedusaLogger
{
    void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, Dictionary<string, object?>? logParams = null);

    void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, Exception ex);

    void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, MedusaError error);

    void LogTechnical(LogLevel logLevel, Exception ex, string message, Dictionary<string, object?>? logParams = null);

    void LogTechnical(LogLevel logLevel, MedusaError error, string message, Dictionary<string, object?>? logParams = null);

    void LogTechnical(LogLevel logLevel, string message, Dictionary<string, object?>? logParams = null);

    IDisposable PushCorrelationIdProperty(string correlationId);
}