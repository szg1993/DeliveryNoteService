using Medusa.Enap.Common.Testing.Database.DevTestEnvPostgres;
using Medusa.Enap.Common.Testing.Database.SQLite;
using Medusa.Enap.Common.Testing.Database.TestContainerPostgres;
using Medusa.Enap.Common.Testing.MessageQueue.Fixtures;
using Medusa.Enap.Common.Testing.TestSuite;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.Database;

public sealed class DatabaseFixture : IDatabaseFixture
{
    private readonly IDatabaseFixture _databaseFixture;
    private static RunMode RunMode => RunMode.InMemory;

    public NullSortingStyle NullSortingStyle => _databaseFixture.NullSortingStyle;

    public bool SupportsCluster => _databaseFixture.SupportsCluster;

    public static Type DbContextType { get; set; } = default!;

    public DatabaseFixture()
    {
        _databaseFixture = RunMode switch
        {
            RunMode.InMemory => new SQLiteDatabaseFixture(DbContextType),
            RunMode.TestContainer => new PostgresDatabaseFixture(DbContextType),
            RunMode.DevTestEnv or _ => new DevTestEnvPostgresFixture()
        };
    }

    #region Global Setup/TearDown

    public Task InitializeAsync() => _databaseFixture.InitializeAsync();

    public Task DisposeAsync() => _databaseFixture.DisposeAsync();

    #endregion
    
    #region Per Test Setup/TearDown

    public Task<string> CreateTestDbAsync(string testId)
        => _databaseFixture.CreateTestDbAsync(testId);

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
        => _databaseFixture.ConfigureTestServices(testId, serviceCollection);

    public Task ClearTestDbAsync(string testId)
        => _databaseFixture.ClearTestDbAsync(testId);

    #endregion
}
