using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Messaging;

/*
 * Mostly AI-generated code
 */

public sealed class SortMap<TModel>
{
    private readonly Dictionary<string, Func<IQueryable<TModel>, SortDirection, IOrderedQueryable<TModel>>> _map
        = new(StringComparer.OrdinalIgnoreCase);

    public SortMap<TModel> Add<TKey>(
        object key,
        Expression<Func<TModel, TKey>> keySelector
    )
    {
        _map[key.ToString()!] = (query, dir) =>
            dir == SortDirection.Desc
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);

        return this;
    }

    public SortMap<TModel> Add<TKey>(
        object key,
        Expression<Func<TModel, TKey>> primary,
        Expression<Func<TModel, TKey>> secondary)
    {
        _map[key.ToString()!] = (query, dir) =>
            dir == SortDirection.Desc
                ? query.OrderByDescending(primary).ThenByDescending(secondary)
                : query.OrderBy(primary).ThenBy(secondary);

        return this;
    }

    public SortMap<TModel> Add(
        string key,
        Func<IQueryable<TModel>, SortDirection, IOrderedQueryable<TModel>> custom)
    {
        _map[key] = custom;
        return this;
    }

    public Func<IQueryable<TModel>, IOrderedQueryable<TModel>> Get(Sorting sorting)
    {
        if (sorting.SortBy is not null && _map.TryGetValue(sorting.SortBy, out var sorter))
            return q => sorter(q, sorting.SortDir ?? SortDirection.Asc);

        return q => q.OrderBy(_ => 0); // no-op that returns IOrderedQueryable
    }
}