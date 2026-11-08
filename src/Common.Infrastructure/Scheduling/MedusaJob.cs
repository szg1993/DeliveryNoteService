using Medusa.Enap.Common.Core.Logging;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Medusa.Enap.Common.Infrastructure.Scheduling;

// Remove LocalLockProvider as DisallowConcurrentExecution handles the same thing
[DisallowConcurrentExecution]
public abstract class MedusaJob(
    MedusaJobContext jobContext
)
    : IJob
{
    protected readonly IMedusaLogger Logger = jobContext.Logger;

    private string? _name;

    public string Name
    {
        get
        {
            _name ??= GetType().Name;

            return _name;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var _ = Logger.PushCorrelationIdProperty(jobContext.CorrelationIdAccessor.CorrelationId);

        try
        {
            Logger.LogTechnical(LogLevel.Information, $"{Name} started.");

            await ExecuteSynchronized(context);

            Logger.LogTechnical(LogLevel.Information, $"{Name} finished.");
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            Logger.LogTechnical(LogLevel.Information, $"{Name} cancelled.");
        }
        catch (Exception ex)
        {
            Logger.LogTechnical(LogLevel.Warning, ex, $"{Name} failed.");
        }
    }

    private async Task ExecuteSynchronized(IJobExecutionContext context)
    {
        using var lockHandle = await jobContext.LockProvider.TryAcquireAsync("jobs:" + Name, context.CancellationToken);
        if (lockHandle is null)
        {
            Logger.LogTechnical(LogLevel.Information, $"{Name} could not acquire lock. Aborting.");
            return;
        }

        if (await jobContext.Ledger.ContainsAsync(context))
        {
            Logger.LogTechnical(LogLevel.Information, $"{Name} for this schedule has already been executed. Aborting.");
            return;
        }

        await RunAsync(context.CancellationToken);

        await jobContext.Ledger.AddAsync(context);
    }

    protected abstract Task RunAsync(CancellationToken ct);
}