using Medusa.Enap.Common.Core.DependencyInjection;
using Medusa.Enap.Common.Core.Infrastructure;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.DemoService.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Medusa.Enap.DemoService.Application.UseCases;

public class DemoJobSettings
{
    public bool Enabled { get; set; }
}

[AutoRegister]
public class IncrementDemoCounter(
    [FromKeyedServices(LocalLockProvider.Key)]
    ILockProvider lockProvider,
    IOptions<DemoJobSettings> settings,
    IRepository<DemoModel> repository,
    IUnitOfWork unitOfWork
)
{
    public Task IncrementClusteredCounterAsync(CancellationToken ct) => RunAsync(m => m.ClusteredJobCounter++, ct);

    public Task IncrementLocalCounterAsync(CancellationToken ct) => RunAsync(m => m.LocalJobCounter++, ct);

    private async Task RunAsync(Action<DemoModel> increment, CancellationToken ct)
    {
        if (!settings.Value.Enabled)
            return;

        using var lockHandle = await lockProvider.AcquireAsync("useCases:" + typeof(IncrementDemoCounter).Name, ct);

        var model = (await repository.GetAllAsync(ct)).FirstOrDefault()
            ?? repository.Add(new DemoModel());

        increment(model);

        await unitOfWork.SaveChangesAsync(ct);
    }
}