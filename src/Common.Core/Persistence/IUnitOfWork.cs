namespace Medusa.Enap.Common.Core.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task<T> RunInTransactionAsync<T>(Func<Task<T>> function, CancellationToken ct);

    Task RunInTransactionAsync(Func<Task> function, CancellationToken ct);

    Task RunInLockingTransactionAsync(Func<Task> function, CancellationToken ct);

    void Reset();
}
