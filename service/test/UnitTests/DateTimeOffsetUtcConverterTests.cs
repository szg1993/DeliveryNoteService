using Common.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnitTests.TestSuite;

namespace UnitTests;

[Collection(SqlUnitTestCollection.Name)]
public class DateTimeOffsetUtcConverterTests : SqlUnitTestBase
{
    public class TestModel
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public DateTimeOffset? NullableTimestamp { get; set; }
    }

    public class TestDbContext(NewEntityLogger logger, DbContextOptions options)
        : AppDbContextBase(logger, options)
    {
        public DbSet<TestModel> TestModels => Set<TestModel>();
    }

    [Fact]
    public async Task SQLite()
    {
        InitSQLite<TestDbContext>();
        await TestDateTimeOffsetUtcConverter();
    }

    [Fact]
    public async Task PostgreSQL()
    {
        await InitPostgresAsync<TestDbContext>();
        await TestDateTimeOffsetUtcConverter();
    }

    private async Task TestDateTimeOffsetUtcConverter()
    {
        DateTimeOffset testTimeStamp = DateTimeOffset.Now.ToHungarianTime();
        await InitDataAsync<TestDbContext>(db =>
        {
            db.TestModels.Add(new()
            {
                Timestamp = testTimeStamp,
                NullableTimestamp = testTimeStamp
            });
        });

        using var dbRead = CreateDbContext<TestDbContext>();
        var testEntity = await dbRead.TestModels.SingleAsync();
        testEntity.Timestamp.Offset.Should().NotBe(testTimeStamp.Offset);
        testEntity.Timestamp.Offset.Should().Be(TimeSpan.Zero);
        testEntity.NullableTimestamp!.Value.Offset.Should().Be(TimeSpan.Zero);
        testEntity.Timestamp.Should().BeCloseTo(testTimeStamp, TimeSpan.FromTicks(10));
        testEntity.NullableTimestamp.Should().BeCloseTo(testTimeStamp, TimeSpan.FromTicks(10));
    }
}