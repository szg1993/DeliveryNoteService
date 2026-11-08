using System.Diagnostics;
using Xunit.Sdk;

namespace Medusa.Enap.Common.Testing.Utils;

public static class Wait
{
    private static readonly TimeSpan _defaultTimeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(3);

    public static Task For(string waitingFor, Func<bool> condition)
        => For(waitingFor, _ => Task.FromResult(condition()));

    public static async Task For(string waitingFor, Func<CancellationToken, Task<bool>> condition)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(_defaultTimeout);

        try
        {
            while (!await condition(cts.Token))
            {
                if (cts.Token.IsCancellationRequested)
                    throw new TaskCanceledException();

                await Task.Delay(10, cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            throw new XunitException($"Waiting for {waitingFor} timed out.");
        }
    }

    public static async Task<TResult> For<TResult>(string waitingFor, Func<CancellationToken, Task<TResult?>> find, Func<string>? getFailureMessage = null)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(_defaultTimeout);

        try
        {
            while (true)
            {
                var result = await find(cts.Token);
                if (result is not null)
                    return result;

                if (cts.Token.IsCancellationRequested)
                    throw new TaskCanceledException();

                await Task.Delay(10, cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            var msg = $"Waiting for {waitingFor} timed out.";
            if (getFailureMessage is not null)
                msg += Environment.NewLine + Environment.NewLine + getFailureMessage();
            throw new XunitException(msg);
        }
    }
}