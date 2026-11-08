using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application;

public enum DemoModelsSortBy
{
    Id, Name
}

public class GetDemoModels(IRepository<DemoModel> r) : IUseCase
{
    private static SortMap<DemoModel> SortMap { get; } = new SortMap<DemoModel>()
        .Add(DemoModelsSortBy.Id, x => x.Id)
        .Add(DemoModelsSortBy.Name, x => x.Name)
        ;

    public async Task<PagedEnvelope<DemoModel>> RunAsync(PageRequest pageRequest, Sorting<DemoModelsSortBy> sorting, CancellationToken ct)
    {
        return await r.Paged(pageRequest, SortMap.Get(sorting), ct);
    }
}
