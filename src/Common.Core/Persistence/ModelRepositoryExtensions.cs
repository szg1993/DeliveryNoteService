using Medusa.Enap.Common.Core.Model;

namespace Medusa.Enap.Common.Core.Persistence;

/// <summary>
/// IRepository<TModel> extension methods, when TModel is IHaveId and has a unique ID
/// </summary>
public static class ModelRepositoryExtensions
{
    public static async Task<bool> ExistsAsync<TModel>(this IReadOnlyRepository<TModel> repository,
        int id, CancellationToken ct
    )
        where TModel : IHaveId
    {
        return 0 < await repository.CountAsync(q => q.Where(m => m.Id == id), ct);
    }

    public static async Task<bool> ExistsAllAsync<TModel>(this IReadOnlyRepository<TModel> repository,
        IEnumerable<int> ids, CancellationToken ct
    )
        where TModel : IHaveId
    {
        var dbIds = (await repository
            .GetAllAsync(q => q
                .Where(m => ids.Contains(m.Id))
                .Select(m => m.Id)
                , ct
            )).ToArray();

        return dbIds.Length == ids.Distinct().Count();
    }

    public static Task<TModel?> GetAsync<TModel>(this IReadOnlyRepository<TModel> repository,
        int id, CancellationToken ct
    )
        where TModel : IHaveId
    {
        return repository.FindAsync(model => model.Id == id, ct);
    }

    public static Task<IEnumerable<TModel>> GetAllAsync<TModel>(this IReadOnlyRepository<TModel> repository,
        IEnumerable<int> ids, CancellationToken ct
    )
        where TModel : IHaveId
    {
        return repository.GetAllAsync(q => q.Where(model => ids.Contains(model.Id)), ct);
    }

    public static async Task DeleteAsync<TModel>(this IRepository<TModel> repository,
        int id, CancellationToken ct
    )
        where TModel : IHaveId
    {
        var model = await repository.GetAsync(id, ct);
        if (model is not null)
            repository.Delete(model);
    }
}