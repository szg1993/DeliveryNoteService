using Medusa.Enap.Common.Core.Model;
using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Persistence;

public static class RepositoryExtensions
{
    public static Task<TModel?> FindAsync<TModel>(
        this IReadOnlyRepository<TModel> repository,
        Expression<Func<TModel, bool>> predicate,
        CancellationToken ct
    )
        => repository.FindAsync(q => q.Where(predicate), ct);

    public static Task<IEnumerable<TModel>> GetAllAsync<TModel>(this IReadOnlyRepository<TModel> repository, CancellationToken ct)
        => repository.GetAllAsync(_ => _, ct);

    public static IReadOnlyRepository<TModel> SortBy<TModel, TKey>(this IReadOnlyRepository<TModel> repository, Expression<Func<TModel, TKey>> keySelector)
        where TModel : class
        => repository.TransformQuery(q => q.OrderBy(keySelector));

    public static IReadOnlyRepository<TModel> Filter<TModel>(this IReadOnlyRepository<TModel> repository, Expression<Func<TModel, bool>> predicate)
        where TModel : class
        => repository.TransformQuery(q => q.Where(predicate));
}

// separate class, so that SonarQube won't fail the QualityGate if the methods are grouped by Guid as TId
// which violates csharpsquid:S4136 "All '...' method overloads should be adjacent."
public static class GuidIdRepositoryExtensions
{
    public static Task<TModel?> FindAsync<TModel>(this IReadOnlyRepository<TModel> repository, Guid id, CancellationToken ct)
        where TModel : IHaveId<Guid>
    {
        return repository.FindAsync((TModel x) => x.Id == id, ct);
    }

    public static Task<bool> AnyAsync<TModel>(this IReadOnlyRepository<TModel> repository, Guid id, CancellationToken ct)
        where TModel : IHaveId<Guid>
    {
        return repository.AnyAsync((TModel x) => x.Id == id, ct);
    }

    /// <summary>
    /// Deletes the entity with the ID but DOES NOT save changes. Returns false if the entity was not found.
    /// </summary>
    public static async Task<bool> DeleteAsync<TModel>(this IRepository<TModel> repository, Guid id, CancellationToken ct)
        where TModel : IHaveId<Guid>
    {
        var m = await repository.FindAsync(id, ct);
        if (m is null)
            return false;

        repository.Delete(m);
        return true;
    }
}