namespace Common.Core.HealthProbes;

public interface IReadinessService
{
    bool IsReady { get; set; }

    Task<bool> WaitForInitializationResultAsync();
}