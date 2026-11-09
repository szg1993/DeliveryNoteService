using Common.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Common.Core.Logging;

#pragma warning disable S6672 // Generic logger injection should match enclosing type
public class MedusaLogger<TLogSource>(ILogger<TLogSource> logger) : IMedusaLogger
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    public void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, Exception ex)
        => Log(logLevel, category, labels, eventId, businessId, businessIdType, ToMedusaError(ex));

    public void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, MedusaError error)
    {
        if (error.ErrorCode == CommonErrorCodes.Unknown && logLevel < LogLevel.Warning)
            logLevel = LogLevel.Warning;

        using var errorCodeProp = CreateErrorCodeProp(error.ErrorCode);

        Log(logLevel, category, labels, $"{eventId}[{error.ErrorCode}]", businessId, businessIdType, error.Data!);
    }

    public void Log(LogLevel logLevel, string category, string[] labels, string eventId, string businessId, string businessIdType, Dictionary<string, object?>? logParams = null)
    {
        var logId = Guid.NewGuid();
        using var logIdProp = LogContext.PushProperty(logger, "LogId", logId);
        using var paramsProp = CreateParamsProp(logParams);
        using var businessIdTypeProp = LogContext.PushProperty(logger, "BusinessIdType", businessIdType ?? "-");
        using var errorDescriptionProp = LogContext.PushProperty(logger, "ErrorDescription", "-");
        // message will be ignored in production, but it is useful for test output so that other log entries will be displayed properly
        // later if there'll be a common log converter that converts every single log entry, on a lower level, to the business format
        // then the test output formatter and this line here can change. Then message here will become useless
        logger.Log(logLevel, "{Category} | {Labels} | {EventId} | {BusinessId}", category, labels, eventId, businessId);
    }

    public void LogTechnical(LogLevel logLevel, Exception ex, string message, Dictionary<string, object?>? logParams = null)
    {
        logParams ??= [];

        LogTechnical(logLevel, ToMedusaError(ex), message, logParams);
    }

    public void LogTechnical(LogLevel logLevel, MedusaError error, string message, Dictionary<string, object?>? logParams = null)
    {
        if (error.ErrorCode == CommonErrorCodes.Unknown && logLevel < LogLevel.Warning)
            logLevel = LogLevel.Warning;

        logParams ??= [];
        foreach (var data in error.Data)
            logParams.TryAdd(data.Key, data.Value);

        using var errorCodeProp = CreateErrorCodeProp(error.ErrorCode);
        LogTechnical(logLevel, message, logParams);
    }

    public void LogTechnical(LogLevel logLevel, string message, Dictionary<string, object?>? logParams = null)
    {
        logParams ??= [];
        logParams.Add("Message", message);
        using var paramsProp = CreateParamsProp(logParams);
        using var categoryProp = logger.PushProperty("Category", logLevel == LogLevel.Error ? "exception" : LogConstants.Technical);
        using var eventIdProp = logger.PushProperty("EventId", LogConstants.Technical);
        // log message is only for testing purposes, see comment above
#pragma warning disable S2629 // Logging templates should be constant
#pragma warning disable CA2254 // Template should be a static expression
        logger.Log(logLevel, "[Technical] " + message);
#pragma warning restore CA2254 // Template should be a static expression
#pragma warning restore S2629 // Logging templates should be constant
    }

    public IDisposable PushCorrelationIdProperty(string correlationId)
        => logger.PushProperty("CorrelationId", correlationId);

    private static MedusaError ToMedusaError(Exception ex)
    {
        var err = new MedusaError(ex);
        err.Data["param"] = ex.Message;
        err.Data["stacktrace"] = ex.StackTrace ?? "no stacktrace";
        AddExceptionDetailsToLog(ex, err.Data!);

        return err;
    }

    private IDisposable? CreateParamsProp(Dictionary<string, object?>? logParams)
    {
        return logParams is not null && logParams.Count != 0
            ? LogContext.PushProperty(logger, "Params", logParams)
            : null;
    }

    private IDisposable CreateErrorCodeProp(string errorCode)
    {
        return LogContext.PushProperty(logger, "ErrorCode", errorCode);
    }

    private static void AddExceptionDetailsToLog(Exception ex, Dictionary<string, object?> logParams)
    {
        logParams["exceptionType"] = ex.GetType();

        if (ex.InnerException is not null)
            logParams["innerExceptions"] = GetInnerExceptionNamesAsString(ex.InnerException);
    }

    private static string GetInnerExceptionNamesAsString(Exception ex)
    {
        var result = new List<string>();
        CollectInnerExceptionName(ex, result);

        return string.Join("->", result);
    }

    private static void CollectInnerExceptionName(Exception? ex, List<string> exceptionNames)
    {
        if (ex is null)
            return;

        exceptionNames.Add(ex.GetType().Name);
        CollectInnerExceptionName(ex.InnerException, exceptionNames);
    }
}