using FluentAssertions;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Infrastructure.Persistence;
using Medusa.Enap.Common.Testing.Database.SQLite;
using Microsoft.EntityFrameworkCore;
using UnitTests.TestSuite;

namespace UnitTests;

[Collection(SqlUnitTestCollection.Name)]
public class CaseInsensitiveComparerTest : SqlUnitTestBase
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    private class TestDbContext(NewEntityLogger logger, DbContextOptions options)
        : AppDbContextBase(logger, options)
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    [Fact]
    public async Task SQLite()
    {
        InitSQLite<TestDbContext>();
        await TestCaseInsensitiveComparisonAsync(new InMemoryEfILikeComparer());
    }

    [Fact]
    public async Task PostgreSQL()
    {
        await InitPostgresAsync<TestDbContext>();
        await TestCaseInsensitiveComparisonAsync(new EfILikeComparer());
    }

    private async Task TestCaseInsensitiveComparisonAsync(ICaseInsensitiveComparer comparer)
    {
        await InitDataAsync<TestDbContext>(db =>
        {
            db.TestEntities.Add(new TestEntity { Id = 1, Text = "alpha" });
            db.TestEntities.Add(new TestEntity { Id = 2, Text = "ALPHA" });
            db.TestEntities.Add(new TestEntity { Id = 3, Text = "XaLpHa" });
            db.TestEntities.Add(new TestEntity { Id = 4, Text = "beta" });
        });

        using var db = CreateDbContext<TestDbContext>();
        var entities = await db.TestEntities
            .Where(comparer.Compare<TestEntity>(e => e.Text, "aLpHa"))
            .Select(e => e.Id)
            .ToArrayAsync();
        entities.Should().BeEquivalentTo([1, 2]);
    }
}
