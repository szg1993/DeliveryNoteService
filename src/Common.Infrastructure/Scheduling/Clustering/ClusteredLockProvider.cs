using Medallion.Threading;
using Medusa.Enap.Common.Core.Infrastructure;

namespace Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;

public class ClusteredLockProvider(IDistributedLockProvider provider) : ILockProvider
{
    public const string Key = "clustered";

    public virtual async Task<IDisposable> AcquireAsync(string lockName, CancellationToken ct)
    {
        return await provider.AcquireLockAsync(lockName, timeout: null, ct);
    }

    public virtual async Task<IDisposable?> TryAcquireAsync(string lockName, CancellationToken ct)
    {
        return await provider.TryAcquireLockAsync(lockName, TimeSpan.Zero, ct);
    }
}
