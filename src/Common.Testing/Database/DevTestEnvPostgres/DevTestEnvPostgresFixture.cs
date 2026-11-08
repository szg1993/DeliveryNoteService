using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.Database.DevTestEnvPostgres;

internal class DevTestEnvPostgresFixture : IDatabaseFixture
{
    private const string _connectionString = "User ID=user;Password=password;Host=localhost;Port=5555;Database=pod_registry;";

    public NullSortingStyle NullSortingStyle => NullSortingStyle.NullLast;
    public bool SupportsCluster => true;

    #region Global Setup/TearDown

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    #endregion

    #region Per Test Setup/TearDown

    public Task<string> CreateTestDbAsync(string testId) => Task.FromResult(_connectionString);

    public Task ClearTestDbAsync(string testId) => Task.CompletedTask;

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        // no op
    }

    #endregion
}
