using Medusa.Enap.Common.Infrastructure.Scheduling;
using Medusa.Enap.DemoService.Application.UseCases;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public class ClusteredDemoJob(
    IncrementDemoCounter useCase,
    ClusteredMedusaJobContext context
)
    : MedusaJob(context)
{
    protected override Task RunAsync(CancellationToken ct) => useCase.IncrementClusteredCounterAsync(ct);
}

public class LocalDemoJob(
    IncrementDemoCounter useCase,
    LocalMedusaJobContext context
)
    : MedusaJob(context)
{
    protected override Task RunAsync(CancellationToken ct) => useCase.IncrementLocalCounterAsync(ct);
}
