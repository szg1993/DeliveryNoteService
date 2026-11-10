using Common.Infrastructure.Persistence;
using Common.Testing.Database;
using Common.Testing.Database.SQLite;
using Common.Testing.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using UnitTests.Logging;

namespace UnitTests.TestSuite;

/// <summary>
/// Run unit tests, using test-container, sequentially to avoid exhausting docker.
/// Later it can be optimized to share the same DB server with different database names,
/// similarly to API tests.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class SqlUnitTestCollection
{
    public const string Name = "SqlUnitTests";
    protected SqlUnitTestCollection() { }
}

public abstract class SqlUnitTestBase : IAsyncLifetime
{
    private PostgreSqlContainer? _postgreSqlContainer;
    private string? _dbFile;
    private SqliteConnection? _inMemConnection;

    protected DbContextOptions? DbOptions;

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_postgreSqlContainer is not null)
            await _postgreSqlContainer.DisposeAsync();

        if (_dbFile is not null)
            await SQLiteUtils.DeleteAsync(_dbFile);

        if (_inMemConnection is not null)
            await _inMemConnection.CloseAsync();
    }

    protected async Task InitPostgresAsync<TDbContext>()
        where TDbContext : AppDbContextBase
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithName("medusa_enap_unit_tests_" + Guid.NewGuid().ToString())
            .WithUsername("sa")
            .WithPassword("Password1")
            .WithDatabase(nameof(NewEntityLoggerTests))
            .WithAutoRemove(true)
            .Build();
        await _postgreSqlContainer.StartAsync();
        DbOptions = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;
    }

    protected void InitSQLite<TDbContext>()
        where TDbContext : AppDbContextBase
    {
        _dbFile = $"{GetType().Name}_{Guid.NewGuid()}.db";

        DbOptions = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(SQLiteUtils.CreateConnectionString(_dbFile))
            .Options;
    }

    protected async Task InitInMemoryAsync<TDbContext>()
        where TDbContext : AppDbContextBase
    {

        _inMemConnection = new SqliteConnection("Data Source=:memory:");
        await _inMemConnection.OpenAsync();

        DbOptions = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(_inMemConnection)
            .Options;
    }

    protected async Task InitDataAsync<TDbContext>(Action<TDbContext>? action = null)
        where TDbContext : AppDbContextBase
    {
        using var context = CreateDbContext<TDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (action is null)
            return;

        action(context);
        await context.SaveChangesAsync();
    }

    protected virtual TDbContext CreateDbContext<TDbContext>()
        where TDbContext : AppDbContextBase
        => CreateDbContext<TDbContext>(new NewEntityLoggerMock().Logger);

    protected TDbContext CreateDbContext<TDbContext>(NewEntityLogger logger)
        where TDbContext : AppDbContextBase
    {
        if (DbOptions == null)
            throw new InvalidOperationException("DbOptions cannot be null. Call");

        var db = Activator.CreateInstance(typeof(TDbContext), logger, DbOptions)
            ?? throw new InvalidOperationException("Failed to construct TestDbContext");
        return (TDbContext)db;
    }
}