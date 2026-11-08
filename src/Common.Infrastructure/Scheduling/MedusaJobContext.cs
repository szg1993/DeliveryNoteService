using Medusa.Enap.Common.Core.Infrastructure;
using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Infrastructure.Scheduling;

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