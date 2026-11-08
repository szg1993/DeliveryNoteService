using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Collections.Concurrent;
using Testcontainers.PostgreSql;

namespace Medusa.Enap.Common.Testing.Database.TestContainerPostgres;

public sealed class PostgresDatabaseFixture(Type dbContextType) : IDatabaseFixture
{
    private const string _templateDbName = "TemplateDb";
    private string _templateDbConnectionString = null!;
    private PostgreSqlContainer? _postgreSqlContainer;
    private readonly ConcurrentDictionary<string, string> _testConnectionStrings = [];

    public NullSortingStyle NullSortingStyle => NullSortingStyle.NullLast;

    public bool SupportsCluster => true;

    #region Global Setup/TearDown

    public async Task InitializeAsync()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithName("medusa_enap_api_tests_" + Guid.NewGuid().ToString())
            .WithUsername("sa")
            .WithPassword("Password1")
            .WithDatabase(_templateDbName)
            .WithAutoRemove(true)
            .Build();

        await _postgreSqlContainer.StartAsync();
        _templateDbConnectionString = _postgreSqlContainer.GetConnectionString();

        await PostgresTemplateDbInitializer.CreateTemplateDbAsync(_templateDbConnectionString, dbContextType);
        NpgsqlConnection.ClearPool(new NpgsqlConnection(_templateDbConnectionString));
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer!.DisposeAsync();
    }

    #endregion

    #region Per Test Setup/TearDown

    public async Task<string> CreateTestDbAsync(string testId)
    {
        var testDbName = $"testDb_{testId}";

        var testDbConnectionString = new NpgsqlConnectionStringBuilder(_templateDbConnectionString)
        {
            Database = testDbName,
            // pooling keeps up to MaxPoolSize connections open per test DB to be reused throughout the test method run
            // will be closed at test teardown.
            Pooling = true,
            MaxPoolSize = 3,
            ConnectionIdleLifetime = 10
        }.ConnectionString;

        if (_testConnectionStrings.TryAdd(testId, testDbConnectionString))
            await PostgresDbCloner.CloneDatabaseAsync(_templateDbConnectionString, testDbName);

        return testDbConnectionString;
    }

    public Task ClearTestDbAsync(string testId)
    {
        // close all connections to the test DB to avoid leaking open connections in the pool
        // they can overflow the overall max open connection count easily
        if (_testConnectionStrings.TryGetValue(testId, out var cs))
            NpgsqlConnection.ClearPool(new NpgsqlConnection(cs));

        return Task.CompletedTask;
    }

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        // no op
    }

    #endregion
}
