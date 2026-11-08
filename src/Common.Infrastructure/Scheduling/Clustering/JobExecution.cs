using Medusa.Enap.Common.Core.Model;

namespace Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;

public class JobExecution : IHaveId
{
    public int Id { get; set; }

    public string JobName { get; set; } = null!;

    public DateTimeOffset? ScheduledFireTime { get; set; }

    public DateTimeOffset FireTime { get; set; }

    public string CorrelationId { get; set; } = null!;
}