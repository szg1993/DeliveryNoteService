using FluentAssertions;
using Common.AspNetCore.Logging;
using Common.Core.Exceptions;
using Common.Core.Integration;
using Common.Core.Logging;
using Common.Testing.Logging;
using Microsoft.Extensions.Logging;
using Serilog;

namespace UnitTests.Logging;

public class MedusaLoggerTests
{
    public class TestLogSourceContext { }

    private readonly DeliveryNoteServiceLogger<MedusaLoggerTests> _logger;
    private LogEntry _logEntry = null!;
    private readonly ILogger<TestLogSourceContext> _externalSystemLogger;
    private readonly Guid _guid = Guid.Parse("5D492C13-86F6-43FC-B0F6-245DE46853DC");
    private readonly CorrelationIdAccessor _correlationIdAccessor = new();

    public MedusaLoggerTests()
    {
        _correlationIdAccessor.CorrelationId = _guid.ToString();
        var lf = new LoggerFactory()
            .AddSerilog(new TestSerilogLogger((e) => _logEntry = e.ToLogEntry()));
        _logger = new DeliveryNoteServiceLogger<MedusaLoggerTests>(new Logger<MedusaLoggerTests>(lf));
        _logger.PushCorrelationIdProperty(_correlationIdAccessor.CorrelationId);
        _externalSystemLogger = new Logger<TestLogSourceContext>(lf);
    }

    [Fact]
    public void TechnicalLog()
    {
        _logger.LogTechnical(LogLevel.Information, "test log message");

        _logEntry.Should().NotBeOfType<BusinessLogEntry>();
        _logEntry.Category.Should().Be("technical");
        _logEntry.EventId.Should().Be("technical");
        _logEntry.CorrelationId.Should().Be(_guid);
        _logEntry.LogLevel.Should().Be("INFO");
        _logEntry.ErrorCode.Should().BeNull();
        _logEntry.Params.Should().Contain("Message", "test log message");
    }

    [Fact]
    public void BusinessLog()
    {
        _logger.Log(LogLevel.Error, "test category", ["label1", "label2"], "test event id", "test business id", "test business id type");

        _logEntry.Should().BeOfType<BusinessLogEntry>();
        _logEntry.Category.Should().Be("test category");
        _logEntry.EventId.Should().Be("test event id");
        _logEntry.CorrelationId.Should().Be(_guid);
        _logEntry.ErrorCode.Should().BeNull();
        _logEntry.Params.Should().BeNull();

        var be = (BusinessLogEntry)_logEntry;
        be.BusinessId.Should().Be("test business id");
        be.BusinessIdType.Should().Be("test business id type");
        be.Labels.Should().BeEquivalentTo("label1", "label2");
    }

    [Fact]
    public void TechnicalLog_Exception()
    {
        _logger.LogTechnical(LogLevel.Error, new MedusaException("test error code", "test exception message").SetData("key", "value"), "test log message");

        _logEntry.Should().NotBeNull();
        _logEntry.Should().NotBeOfType<BusinessLogEntry>();
        _logEntry!.Category.Should().Be("exception");
        _logEntry!.CorrelationId.Should().Be(_guid);
        _logEntry!.ErrorCode.Should().Be("test error code");
        _logEntry!.Params.Should().Contain("key", "value");
        _logEntry!.Params.Should().Contain("exceptionType", "Common.Core.Exceptions.MedusaException");
        _logEntry!.Params.Should().NotContainKey("innerExceptions");
    }

    [Fact]
    public void TechnicalLog_InnerException()
    {
        var innerException = new InvalidOperationException(message: null, new ArgumentOutOfRangeException(message: null, new NotImplementedException()));
        _logger.LogTechnical(LogLevel.Error, new MedusaException("dummy", "dummy", innerException), "dummy");

        _logEntry!.Params.Should().Contain("innerExceptions", "InvalidOperationException->ArgumentOutOfRangeException->NotImplementedException");
    }

    [Fact]
    public void BusinessLog_Exception()
    {
        _logger.Log(LogLevel.Error, "test category", [], "test event id", "test business id", "test business id type",
            new MedusaException("test error code", "test exception message").SetData("key", "value")
        );

        _logEntry.Should().NotBeNull();
        _logEntry.Should().BeOfType<BusinessLogEntry>();
        _logEntry!.EventId.Should().Be("test event id[test error code]");
        _logEntry!.Category.Should().Be("test category");
        _logEntry!.EventId.Should().Be("test event id[test error code]");
        _logEntry!.CorrelationId.Should().Be(_guid);
        _logEntry!.ErrorCode.Should().Be("test error code");
        _logEntry!.Params.Should().Contain("param", "test exception message");
        _logEntry!.Params.Should().Contain("stacktrace", "no stacktrace");
        _logEntry!.Params.Should().Contain("key", "value");
        _logEntry!.Params.Should().NotContainKey("innerExceptions");
        var be = (BusinessLogEntry)_logEntry;
        be.BusinessId.Should().Be("test business id");
        be.BusinessIdType.Should().Be("test business id type");
    }

    [Fact]
    public void BusinessLog_InnerException()
    {
        var innerException = new InvalidOperationException(message: null, new ArgumentOutOfRangeException(message: null, new NotImplementedException()));
        _logger.Log(LogLevel.Error, "dummy", [], "dummy", "dummy", "dummy", new MedusaException("dummy", "dummy", innerException));

        _logEntry!.Params.Should().Contain("innerExceptions", "InvalidOperationException->ArgumentOutOfRangeException->NotImplementedException");
    }

    [Fact]
    public void CorrelationId_IsLogged_WithExternalLogs()
    {
        _externalSystemLogger.Log(LogLevel.Information, "Test external log message");

        _logEntry.CorrelationId.Should().Be(_guid);
    }
}