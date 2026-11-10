namespace Common.Core.Utils;

public static class TaskExtensions
{
    public static async Task Then<TResult>(this Task<TResult> task, Action<TResult> action)
    {
        var result = await task;
        action(result);
    }

    public static async Task Then<TResult>(this Task<TResult> task, Func<TResult, Task> asyncAction)
    {
        var result = await task;
        await asyncAction(result);
    }

    public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, TNewResult> func)
    {
        var result = await task;
        return func(result);
    }

    public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, Task<TNewResult>> asyncFunc)
    {
        var result = await task;
        return await asyncFunc(result);
    }
}