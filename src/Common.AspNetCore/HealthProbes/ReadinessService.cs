using Medusa.Enap.Common.Core.HealthProbes;

namespace Medusa.Enap.Common.AspNetCore.HealthProbes;

public class ReadinessService : IReadinessService
{
    private bool _isReady = false;
    private readonly TaskCompletionSource<bool> _tcsInitialization = new();

    public bool IsReady
    {
        get => _isReady;
        set
        {
            _isReady = value;
            _tcsInitialization.TrySetResult(value);
        }
    }

    public Task<bool> WaitForInitializationResultAsync() => _tcsInitialization.Task;
}
