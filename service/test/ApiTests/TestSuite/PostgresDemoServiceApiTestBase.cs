using Medusa.Enap.Common.Testing.Database;
using Medusa.Enap.Common.Testing.Database.TestContainerPostgres;
using Medusa.Enap.DemoService.Persistence;

namespace ApiTests.TestSuite;

public abstract class PostgresDemoServiceApiTestBase : DemoServiceApiTestBase
{
    private readonly PostgresDatabaseFixture _database = new(typeof(AppDbContext));

    protected override IDatabaseFixture TestDatabase => _database;

    public override async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            await _database.DisposeAsync();
        }
    }
}