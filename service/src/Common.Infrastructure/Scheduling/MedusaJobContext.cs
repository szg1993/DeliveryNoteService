using Common.Core.Infrastructure;
using Common.Core.Integration;
using Common.Core.Logging;
using Common.Infrastructure.Scheduling.Clustering;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Scheduling;

public class MedusaJobContext(
    ILockProvider lockProvider,
    IJobExecutionLedger ledger,
    IMedusaLogger logger,
    CorrelationIdAccessor correlationIdAccessor
)
{
    public ILockProvider LockProvider => lockProvider;
    public IMedusaLogger Logger => logger;
    public CorrelationIdAccessor CorrelationIdAccessor => correlationIdAccessor;
    public IJobExecutionLedger Ledger => ledger;
}

public class LocalMedusaJobContext(
    [FromKeyedServices(LocalLockProvider.Key)]
    ILockProvider lockProvider,
    MedusaLogger<MedusaJob> logger,
    CorrelationIdAccessor correlationIdAccessor
) : MedusaJobContext(lockProvider, new BlackHoleJobExecutionLedger(), logger, correlationIdAccessor);

public class ClusteredMedusaJobContext(
    [FromKeyedServices(ClusteredLockProvider.Key)]
    ILockProvider lockProvider,
    PersistentJobExecutionLedger ledger,
    MedusaLogger<MedusaJob> logger,
    CorrelationIdAccessor correlationIdAccessor
) : MedusaJobContext(lockProvider, ledger, logger, correlationIdAccessor);