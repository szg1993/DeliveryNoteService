using Medusa.Enap.Common.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.Core.Logging;

public enum LogBeginEvent
{
    /// <summary>
    /// "{EventName}"
    /// </summary>
    Start,
    /// <summary>
    /// "{EventName}Run"
    /// </summary>
    Run,
    /// <summary>
    /// "{EventName}Request
    /// </summary>
    Request
}

public enum LogEndEvent
{
    /// <summary>
    /// "{EventName}Success"
    /// </summary>
    Success,
    /// <summary>
    /// "{EventName}Successful"
    /// </summary>
    Successful
}

#pragma warning disable S6672 // Generic logger injection should match enclosing type
public class BusinessEventLogger<TUseCase>(
    ILogger<TUseCase> logger,
    IRequestContext requestContext
)
    : MedusaLogger<TUseCase>(logger), IBusinessEventLogger
    where TUseCase : class
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    private string _category = null!;
    private string[] _labels = null!;
    private string _eventId = string.Empty;

    private const string _runSuffix = "Run";
    private const string _successSuffix = "Success";
    private const string _errorSuffix = "Error";

    private readonly Dictionary<string, string> _businessIds = [];
    private readonly Dictionary<string, object?> _logParams = [];

    public BusinessEventLogger<TUseCase> Init(string category, string[] labels, string? eventIdPrefix = null)
    {
        _category = category;
        _labels = labels;
        _eventId = eventIdPrefix ?? string.Empty;

        return this;
    }

    public BusinessEventLogger<TUseCase> WithBusinessId(string businessIdType, string businessId)
    {
        _businessIds[businessIdType] = businessId;
        return this;
    }

    public BusinessEventLogger<TUseCase> WithLogParams(Dictionary<string, object?> logParams)
    {
        foreach (var param in logParams)
            _ = WithLogParam(param.Key, param.Value);

        return this;
    }

    public BusinessEventLogger<TUseCase> WithLogParam(string key, object? value)
    {
        _logParams[key] = value;
        return this;
    }

    public BusinessEventLogger<TUseCase> RemoveBusinessId(string businessIdType)
    {
        _businessIds.Remove(businessIdType);
        return this;
    }

    public Task RunAsync(
        LogBeginEvent begin, LogEndEvent end,
        Func<Task> operation
    ) => RunAsync(begin, end, _logParams, operation);

    public Task RunAsync(
        LogBeginEvent begin, LogEndEvent end,
        Dictionary<string, object?> logParams,
        Func<Task> operation
    ) => RunAsync(begin, end, logParams, WrapTaskVoid(operation));

    public Task<TResult> RunAsync<TResult>(
        LogBeginEvent begin, LogEndEvent end,
        Func<Task<TResult>> operation
    ) => RunAsync(begin, end, _logParams, operation);

    public async Task<TResult> RunAsync<TResult>(
        LogBeginEvent begin, LogEndEvent end,
        Dictionary<string, object?> logParams,
        Func<Task<TResult>> operation
    )
    {
        // note: copy the original logParams, because the first call to LogTechnical might modify it
        //       and there'll be DuplicateKeyExceptions. MedusaLogger should copy the dictionary in the future.
        try
        {
            if (begin == LogBeginEvent.Start)
                LogBusinessEventStart(requestContext.ToLogParams().Add(logParams));
            else
                LogBusinessEvent($"{_eventId}{begin}", requestContext.ToLogParams().Add(logParams));

            var r = await operation();

            if (end == LogEndEvent.Success)
                LogBusinessEventSuccess(new(logParams));
            else
                LogBusinessEventSuccessful(new(logParams));

            return r;
        }
        catch (Exception ex)
        {
            LogBusinessEventWarning(ex);
            throw;
        }
    }

    public void LogBusinessEventStart(Dictionary<string, object?>? logParams = null)
        => LogBusinessEvent(_eventId, logParams);

    public void LogBusinessEventRun(Dictionary<string, object?>? logParams = null)
        => LogBusinessEvent(_eventId + _runSuffix, logParams);

    public void LogBusinessEventSuccess(Dictionary<string, object?>? logParams = null)
        => LogBusinessEvent(_eventId + _successSuffix, logParams: logParams);

    public void LogBusinessEventSuccessful(Dictionary<string, object?>? logParams = null)
        => LogBusinessEvent(_eventId + _successSuffix + "ful", logParams: logParams);

    public void LogBusinessEventError(Exception ex)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Error, _category, _labels, _eventId + _errorSuffix, businessId, businessIdType, ex);
    }

    public void LogBusinessEventError(string eventId, Exception ex)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Error, _category, _labels, eventId, businessId, businessIdType, ex);
    }

    public void LogBusinessEventError(string eventId, MedusaError err)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Warning, _category, _labels, eventId, businessId, businessIdType, err);
    }

    public void LogBusinessEventError(string eventId, Dictionary<string, object?>? logParams = null)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Error, _category, _labels, eventId, businessId, businessIdType, logParams);
    }

    public void LogBusinessEventWarning(string eventId, Dictionary<string, object?>? logParams = null)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Warning, _category, _labels, eventId, businessId, businessIdType, logParams);
    }

    public void LogBusinessEventWarning(Exception ex)
    {
        LogBusinessEventWarning(_eventId + _errorSuffix, ex);
    }

    public void LogBusinessEventWarning(string eventId, Exception ex)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(LogLevel.Warning, _category, _labels, eventId, businessId, businessIdType, ex);
    }

    public void LogBusinessEvent(string? eventId = null, Dictionary<string, object?>? logParams = null, LogLevel? logLevel = null)
    {
        foreach (var (businessIdType, businessId) in _businessIds)
            Log(logLevel ?? LogLevel.Information, _category, _labels, eventId ?? _eventId, businessId, businessIdType, logParams);
    }

    private static Func<Task<bool>> WrapTaskVoid(Func<Task> operation)
    {
        return async () =>
        {
            await operation();
            return true;
        };
    }
}
