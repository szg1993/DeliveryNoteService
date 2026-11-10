using Common.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Common.Testing.Infrastructure;

public class TestTimeService : TimeService
{
    private DateTimeOffset _time = DateTimeOffset.UtcNow;
    private readonly ConcurrentDictionary<string, TaskCompletionSource> _delays = [];

    public TestTimeService AdvanceMinutes(int minutes) => Advance(TimeSpan.FromMinutes(minutes));

    public TestTimeService Advance(TimeSpan timeSpan)
        => SetDateTime(_time.Add(timeSpan));

    /// <summary>
    /// Sets the current date.
    /// Also sets the time to 12:00 (noon) to avoid accidentally switching date in UTC/Hungarian timezones
    /// if the tests are run around midnight.
    /// </summary>
    public TestTimeService SetDate(DateOnly newDate)
        => SetDateTime(new DateTimeOffset(newDate, new TimeOnly(12, 00), _time.Offset));

    public TestTimeService SetTime(TimeOnly newTime)
        => SetDateTime(new DateTimeOffset(_time.ToDateOnly(), newTime, _time.Offset));

    public TestTimeService SetDateTime(DateTimeOffset newDateTime)
    {
        _time = newDateTime;
        return this;
    }

    public bool HasDelay(string delayId) => _delays.ContainsKey(delayId);

    public void TriggerDelay(string delayId)
    {
        if (_delays.Remove(delayId, out var tcs))
            tcs.SetResult();
    }

    #region ITimeService members

    public override DateTimeOffset UtcNow => _time;

    /// <summary>
    /// Returns the Hungarian time zone offset to UTC on the current date of TimeService.UtcNow
    /// </summary>
    public TimeSpan HungarianOffset => DateTimeExtensions.GetHungarianOffset(UtcNow);

    public override Task Delay(string delayId, TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        var tcs = _delays.GetOrAdd(delayId, _ => new());
        // once the test is finished and the WebApp torn down, then these delays must be cancelled
        // otherwise some awaiting threads might get stuck
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        return tcs.Task;
    }

    #endregion
}

public static class TestTimeServiceHelper
{
    public static IServiceCollection AddTestTimeService(this IServiceCollection services, ITimeService ts)
        => services.AddSingleton(ts);

    public static IServiceCollection AddTestTimeService(this IServiceCollection services)
        => services.AddSingleton<ITimeService, TestTimeService>();

    public static TestTimeService Time(this IScenario scenario)
        => scenario.Services.GetTestTimeService();

    public static TestTimeService GetTestTimeService(this IServiceProvider services)
        => services.GetRequiredService<ITimeService>() as TestTimeService
            ?? throw new InvalidOperationException("TestTimeService is not registered");
}