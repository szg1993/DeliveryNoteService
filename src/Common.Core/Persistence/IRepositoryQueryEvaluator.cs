using Medusa.Enap.Common.Core.Messaging;

namespace Medusa.Enap.Common.Core.Persistence;

public interface IRepositoryQueryEvaluator
{
    Task<int> CountAsync<T>(IQueryable<T> q, CancellationToken ct);

    Task<TResult[]> GetAllAsync<TResult>(IQueryable<TResult> q, CancellationToken ct);

    Task<(PageDetails page, TResult[] result)> GetPagedAsync<TResult>(
        IQueryable<TResult> query,
        PageRequest pageRequest,
        CancellationToken ct
    );
}