using Common.Testing.Database;
using Common.Testing.Database.TestContainerPostgres;
using DeliveryNoteService.Persistence;

namespace ApiTests.TestSuite;

public abstract class PostgresDeliveryNoteServiceApiTestBase : DeliveryNoteServiceApiTestBase
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