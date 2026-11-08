namespace Medusa.Enap.Common.Core.Infrastructure;

public interface ITimeService
{
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Current date in Hungarian time zone UTC+1 CET and Summer time UTC+2 CEST.
    /// </summary>
    DateOnly Today { get; }

    Task Delay(string delayId, TimeSpan timeSpan, CancellationToken cancellationToken);
}

public class TimeService : ITimeService
{
    virtual public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateOnly Today => UtcNow.ToHungarianTime().ToDateOnly();

    virtual public Task Delay(string delayId, TimeSpan timeSpan, CancellationToken cancellationToken) 
        => Task.Delay(timeSpan, cancellationToken);
}
