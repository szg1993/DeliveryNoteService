using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Persistence;
using Quartz;

namespace Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;

public interface IJobExecutionLedger
{
    Task AddAsync(IJobExecutionContext context);
    Task<bool> ContainsAsync(IJobExecutionContext context);
}

public class BlackHoleJobExecutionLedger() : IJobExecutionLedger
{
    public Task AddAsync(IJobExecutionContext context) => Task.CompletedTask;

    public Task<bool> ContainsAsync(IJobExecutionContext context) => Task.FromResult(false);
}

public class PersistentJobExecutionLedger(
    IRepository<JobExecution> repo,
    IUnitOfWork unitOfWork,
    CorrelationIdAccessor correlationIdAccessor
) : IJobExecutionLedger
{
    public async Task AddAsync(IJobExecutionContext context)
    {
        repo.Add(new JobExecution()
        {
            JobName = context.JobDetail.Key.ToString(),
            ScheduledFireTime = context.ScheduledFireTimeUtc,
            FireTime = context.FireTimeUtc,
            CorrelationId = correlationIdAccessor.CorrelationId
        });

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    public Task<bool> ContainsAsync(IJobExecutionContext context)
    {
        if (context.ScheduledFireTimeUtc is null)
            return Task.FromResult(false); // non-scheduled task, do not try to find a match

        return repo.AnyAsync(j => 
            j.ScheduledFireTime == context.ScheduledFireTimeUtc && 
            j.JobName == context.JobDetail.Key.ToString()
        , context.CancellationToken);
    }
}
