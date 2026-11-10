using System.Collections.Concurrent;

namespace Common.Core.Infrastructure;

public class LocalLockProvider : ILockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public const string Key = "local";

    public async Task<IDisposable> AcquireAsync(string lockName, CancellationToken ct)
    {
        var semaphore = _locks.GetOrAdd(lockName, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        return new LockHandle(semaphore);
    }

    public async Task<IDisposable?> TryAcquireAsync(string lockName, CancellationToken ct)
    {
        var semaphore = _locks.GetOrAdd(lockName, _ => new SemaphoreSlim(1, 1));
        if (!await semaphore.WaitAsync(0, ct))
            return null;

        return new LockHandle(semaphore);
    }

    private sealed class LockHandle(SemaphoreSlim? semaphore) : IDisposable
    {
        ~LockHandle()
        {
            semaphore?.Release();
            semaphore = null;
        }

        public void Dispose()
        {
            semaphore?.Release();
            semaphore = null;
            GC.SuppressFinalize(this);
        }
    }
}