namespace Medusa.Enap.Common.Core.Infrastructure;

/// <summary>
/// WIP - LockProvider concept is a work in progress. We'll see later how general they will generalize or not.
/// </summary>
public interface ILockProvider
{
    Task<IDisposable> AcquireAsync(string lockName, CancellationToken ct);

    Task<IDisposable?> TryAcquireAsync(string lockName, CancellationToken ct);
}
