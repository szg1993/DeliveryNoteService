using Common.AspNetCore.Logging;
using Common.Core.Logging;
using FluentAssertions;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Common.Testing.Logging;

public class TestLogSink(TestLogStore testLogStore) : ILogEventSink
{
    public void Emit(LogEvent logEvent) => testLogStore.Add(logEvent);
}

public class TestLogStore()
{
    private readonly ConcurrentQueue<LogEntry> _logEntries = [];

    public IReadOnlyCollection<LogEntry> Entries => _logEntries;

    internal void Add(LogEvent logEvent)
    {
        _logEntries.Enqueue(logEvent.ToLogEntry());
    }

    public void AssertNoErrors(string errorCode)
    {
        AssertNoErrors(err => err.ErrorCode == errorCode);
    }

    public void AssertNoErrors(Expression<Func<LogEntry, bool>>? predicate = null)
    {
        predicate ??= _ => true;
        GetErrors().Should().NotContain(predicate);
    }

    public LogEntry AssertError(string errorCode, string dataKey, string dataValue)
    {
        return AssertError(errorCode, (dataKey, dataValue));
    }

    public LogEntry AssertError(string errorCode, params (string key, string value)[] expectedErrorData)
    {
        return AssertError(errorCode, l =>
        {
            if (expectedErrorData.Length == 0)
                return true;
            if (l.Params is null)
                return false;
            return expectedErrorData.All(e => l.Params.TryGetValue(e.key, out var val) && val == e.value);
        });
    }

    public LogEntry AssertError(string errorCode, Func<LogEntry, bool> predicate)
    {
        var combinedPredicate = (LogEntry l) => l.ErrorCode == errorCode && predicate(l);

        var errors = GetErrors();
        errors.Should().Contain(l => combinedPredicate(l));
        return errors.Last(combinedPredicate);
    }

    public LogEntry AssertTechnicalEntry(string expectedMessage)
    {
        var predicate = (LogEntry l) =>
            l.Category == LogConstants.Technical &&
            l.Params != null && l.Params["Message"] == expectedMessage;

        _logEntries.Should().Contain(l => predicate(l));
        return _logEntries.Last(predicate);
    }

    public void AssertNoTechnicalEntry(string expectedMessage)
    {
        GetTechnicalEntries(expectedMessage).Should().BeEmpty();
    }

    public BusinessLogEntry AssertBusinessEntry(string category, string[] labels, string eventId, string businessId, string businessIdType)
    {
        var predicate = (BusinessLogEntry l) =>
            l.Category == category &&
            new HashSet<string>(l.Labels).SetEquals(labels) &&
            l.EventId == eventId &&
            l.BusinessId == businessId &&
            l.BusinessIdType == businessIdType;

        var businessLogs = _logEntries.OfType<BusinessLogEntry>();
        businessLogs.Should().Contain(l => predicate(l));
        return businessLogs.Last(predicate);
    }

    public void AssertNoBusinessEntry(string category, string[] labels, string eventId, string businessId, string businessIdType)
    {
        GetBusinessEntries(category, labels, eventId, businessId, businessIdType).Should().BeEmpty();
    }

    public LogEntry? GetError(string errorCode, Func<LogEntry, bool>? predicate = null)
    {
        predicate ??= _ => true;
        return Entries.FirstOrDefault(l => l.ErrorCode == errorCode && predicate(l));
    }

    public IEnumerable<LogEntry> GetErrors()
    {
        return Entries.Where(le => !string.IsNullOrEmpty(le.ErrorCode));
    }

    public LogEntry? GetTechnicalEntry(string expectedMessage)
    {
        return _logEntries.FirstOrDefault(
            le => le.Category == LogConstants.Technical && le.Params?["Message"] == expectedMessage
        );
    }

    public IEnumerable<LogEntry> GetTechnicalEntries(string expectedMessage)
    {
        return _logEntries.Where(
            le => le.Category == LogConstants.Technical && le.Params?["Message"] == expectedMessage
        );
    }

    public BusinessLogEntry? GetBusinessEntry(string category, string[] labels, string eventId, string businessId, string businessIdType)
    {
        return _logEntries
            .OfType<BusinessLogEntry>()
            .FirstOrDefault(ble =>
                ble.Category == category &&
                new HashSet<string>(ble.Labels).SetEquals(labels) &&
                ble.EventId! == eventId &&
                ble.BusinessId == businessId &&
                ble.BusinessIdType == businessIdType
        );
    }

    public IEnumerable<BusinessLogEntry> GetBusinessEntries(string category, string[] labels, string eventId, string businessId, string businessIdType)
    {
        return _logEntries
            .OfType<BusinessLogEntry>()
            .Where(ble =>
                ble.Category == category &&
                new HashSet<string>(ble.Labels).SetEquals(labels) &&
                ble.EventId! == eventId &&
                ble.BusinessId == businessId &&
                ble.BusinessIdType == businessIdType
        );
    }
}

public static class LogEntryExtensions
{
    public static LogEntry AssertParam(this LogEntry logEntry, string key, string expectedValue)
    {
        return AssertParam(logEntry, key, v => v.Should().Be(expectedValue));
    }

    public static LogEntry AssertParam(this LogEntry logEntry, string key, Action<string?> assertion)
    {
        logEntry.Params.Should().NotBeNull();
        assertion(logEntry.Params![key]);
        return logEntry;
    }

    public static LogEntry AssertLevel(this LogEntry logEntry, string expectedLevel)
    {
        logEntry.LogLevel.Should().Be(expectedLevel);
        return logEntry;
    }
}