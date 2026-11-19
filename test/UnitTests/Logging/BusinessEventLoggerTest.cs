using Common.AspNetCore.Logging;
using Common.Core.Exceptions;
using Common.Core.Logging;
using Common.Testing.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Events;

namespace UnitTests.Logging;

public class BusinessEventLoggerTest
{
    private readonly BusinessEventLogger<BusinessEventLoggerTest> _businessEventLogger;
    private readonly List<LogEntry> _logEntries = [];

    private readonly IRequestContext _requestContext;

    public BusinessEventLoggerTest()
    {
        var m = new Mock<IRequestContext>();
        m.Setup(m => m.ToLogParams()).Returns(LogParams.Create("TestKey", "TestValue"));
        _requestContext = m.Object;

        var lf = new LoggerFactory()
            .AddSerilog(new TestSerilogLogger((e) => _logEntries.Add(e.ToLogEntry())));
        _businessEventLogger = new BusinessEventLogger<BusinessEventLoggerTest>(
            new Logger<BusinessEventLoggerTest>(lf),
            _requestContext
        );
        _businessEventLogger
           .Init("test category", ["test label 1", "test label 2"], "test event")
           .WithBusinessId("test businessid type", "test businessid");
    }

    [Fact]
    public void BusinessEventLogger_LogStart()
    {
        _businessEventLogger.LogBusinessEventStart();

        AssertBusinessLogEntry("test event");
    }

    [Fact]
    public void BusinessEventLogger_LogSuccess()
    {
        _businessEventLogger.LogBusinessEventSuccess();

        AssertBusinessLogEntry("test eventSuccess");
    }

    [Fact]
    public void BusinessEventLogger_LogRun()
    {
        _businessEventLogger.LogBusinessEventRun();

        AssertBusinessLogEntry("test eventRun");
    }

    [Fact]
    public void BusinessEventLogger_Error()
    {
        _businessEventLogger.LogBusinessEventError(new MedusaException("test error code", "test exception message").SetData("key", "value"));

        AssertBusinessLogEntry("test eventError[test error code]")
            .AssertParam("param", "test exception message");
    }

    [Fact]
    public void MultipleBusinessIds()
    {
        _businessEventLogger
            .WithBusinessId("id type 1", "id 1")
            .WithBusinessId("id type 2", "id 2")
            .WithBusinessId("id type 2", "id 2 overwrite")
            .WithBusinessId("id type 3", "id 3")
            .RemoveBusinessId("id type 3");

        _businessEventLogger.LogBusinessEventStart();

        _logEntries.Should().BeEquivalentTo([
            new { BusinessIdType = "test businessid type", BusinessId = "test businessid" },
            new { BusinessIdType = "id type 1", BusinessId = "id 1" },
            new { BusinessIdType = "id type 2", BusinessId = "id 2 overwrite" }
        ]);
    }

    [Fact]
    public async Task RunAsync()
    {
        await _businessEventLogger
            .RunAsync(LogBeginEvent.Run, LogEndEvent.Successful, () => Task.CompletedTask);

        _logEntries.Should().HaveCount(2);
        _logEntries.Where(l => l.EventId == "test eventRun")
            .Should().ContainSingle()
            .Which.Params.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>("testKey", "TestValue"));
        _logEntries.Where(l => l.EventId == "test eventSuccessful")
            .Should().ContainSingle();
    }

    private BusinessLogEntry AssertBusinessLogEntry(string eventId)
    {
        var logEntry = _logEntries.Single();
        logEntry.Should().BeOfType<BusinessLogEntry>();
        logEntry.Category.Should().Be("test category");
        var be = (BusinessLogEntry)logEntry;
        be.BusinessIdType.Should().Be("test businessid type");
        be.BusinessId.Should().Be("test businessid");
        be.Labels.Should().BeEquivalentTo("test label 1", "test label 2");
        be.EventId.Should().Be(eventId);
        Assert.True(Guid.TryParse(be.LogId, out _));
        return be;
    }
}

file class TestSerilogLogger(Action<LogEvent> eventHandler) : Serilog.ILogger
{
    public void Write(LogEvent logEvent) => eventHandler(logEvent);
}