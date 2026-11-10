using Microsoft.Extensions.Logging;

namespace Common.Core.Logging;

#pragma warning disable S6672 // Generic logger injection should match enclosing type
public class TechnicalUseCaseLogger<TUseCase>(
    ILogger<TUseCase> logger,
    IRequestContext requestContext)
    : MedusaLogger<TUseCase>(logger)
#pragma warning restore S6672 // Generic logger injection should match enclosing type
{
    public T Run<T>(Func<T> operation)
        => Run([], operation);

    public T Run<T>(Dictionary<string, object?> logParams, Func<T> operation)
    {
        return RunAsync(logParams, () =>
        {
            var result = operation();
            return Task.FromResult(result);
        }).Result;
    }

    public Task RunAsync(Func<Task> operation)
        => RunAsync([], operation);

    public Task RunAsync(Dictionary<string, object?> logParams, Func<Task> operation)
    {
        return RunAsync(logParams, async () =>
        {
            await operation();
            return true;
        });
    }

    public Task<T> RunAsync<T>(Func<Task<T>> operation)
        => RunAsync([], operation);

    public async Task<T> RunAsync<T>(Dictionary<string, object?> logParams, Func<Task<T>> operation)
    {
        // note: copy the original logParams, because the first call to LogTechnical might modify it
        //       and there'll be DuplicateKeyExceptions. MedusaLogger should copy the dictionary in the future.
        var useCaseName = GetType().GenericTypeArguments[0].Name;
        try
        {
            LogTechnical(LogLevel.Information, $"{useCaseName} started.", requestContext.ToLogParams().Add(logParams));
            var result = await operation();
            LogTechnical(LogLevel.Information, $"{useCaseName} finished.", new(logParams));
            return result;
        }
        catch (Exception ex)
        {
            LogTechnical(LogLevel.Warning, ex, $"{useCaseName} failed.", new(logParams));
            throw;
        }
    }
}