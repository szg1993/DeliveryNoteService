using FluentAssertions;
using Medusa.Enap.Common.AspNetCore.Logging;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Core.Model;
using Medusa.Enap.Common.Infrastructure.Persistence;
using Medusa.Enap.Common.Testing.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using UnitTests.TestSuite;

namespace UnitTests.Logging;

[Collection(SqlUnitTestCollection.Name)]
public class NewEntityLoggerTests : SqlUnitTestBase
{
    private readonly MedusaLogger<NewEntityLogger> _logger;
    private LogEntry _logEntry = null!;
    private readonly NewEntityLogger _newEntityLogger;

    public NewEntityLoggerTests()
    {
        var lf = new LoggerFactory()
           .AddSerilog(new TestSerilogLogger((e) => _logEntry = e.ToLogEntry()));
        _logger = new MedusaLogger<NewEntityLogger>(new Logger<NewEntityLogger>(lf));
        _newEntityLogger = new(_logger);
    }

    private class TestModel : IModel
    {
        public int Id { get; set; }
    }

    private class TestDbContext(NewEntityLogger logger, DbContextOptions options)
        : AppDbContextBase(logger, options)
    {
        public DbSet<TestModel> TestEntities => Set<TestModel>();
    }

    [Fact]
    public async Task SQLite()
    {
        InitSQLite<TestDbContext>();
        await TestLogNewEntity();
    }

    [Fact]
    public async Task PostgreSQL()
    {
        await InitPostgresAsync<TestDbContext>();
        await TestLogNewEntity();
    }

    protected override TDbContext CreateDbContext<TDbContext>()
    {
        return CreateDbContext<TDbContext>(_newEntityLogger);
    }

    private async Task TestLogNewEntity()
    {
        await InitDataAsync<TestDbContext>(db =>
        {
            db.TestEntities.Add(new());
        });

        _logEntry.Should().NotBeNull();
        _logEntry?.Params?["Message"].Should().Be("New TestModel has been saved. Id: 1");
    }
}