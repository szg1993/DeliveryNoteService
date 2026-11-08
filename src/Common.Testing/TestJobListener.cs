using Quartz;

namespace Medusa.Enap.Common.Testing;

internal sealed class TestJobListener(JobKey jobKey) : IJobListener
{
    public string Name => "TestJobListener";

    private readonly TaskCompletionSource<bool> _tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        if (context.JobDetail.Key.Equals(jobKey))
        {
            // Optionally assert jobException == null here or propagate it.
            _tcs.TrySetResult(true);
        }

        return Task.CompletedTask;
    }

    public Task WaitAsync(CancellationToken ct) => _tcs.Task.WaitAsync(ct);
}
