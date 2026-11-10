using Common.Core.DependencyInjection;
using Common.Core.Messaging;
using Common.Core.Persistence;
using Common.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Persistence;

[AutoRegister(ServiceLifetime.Scoped, typeof(IRepositoryQueryEvaluator))]
public class RepositoryQueryEvaluator : IRepositoryQueryEvaluator
{
    public Task<int> CountAsync<T>(IQueryable<T> q, CancellationToken ct)
        => q.CountAsync(ct);

    public Task<TResult[]> GetAllAsync<TResult>(IQueryable<TResult> q, CancellationToken ct)
        => q.ToArrayAsync(ct);

    public Task<(PageDetails page, TResult[] result)> GetPagedAsync<TResult>(
        IQueryable<TResult> query, PageRequest pageRequest, CancellationToken ct
    )
    {
        if (pageRequest.Limit == 0)
            return GetUnpagedAsync(query, ct);

        return GetPageAsync(query, pageRequest, ct);
    }

    private async Task<(PageDetails page, TResult[] result)> GetUnpagedAsync<TResult>(
        IQueryable<TResult> query, CancellationToken ct
    )
    {
        var m = await GetAllAsync(query, ct);
        return (PageDetails.Unpaged(m.Length), m);
    }

    private async Task<(PageDetails page, TResult[] result)> GetPageAsync<TResult>(
        IQueryable<TResult> query, PageRequest pageRequest, CancellationToken ct
    )
    {
        var totalCount = await CountAsync(query, ct);

        var models = await GetAllAsync(query.ApplyPaging(pageRequest), ct);

        return (new PageDetails(pageRequest, totalCount), models);
    }
}