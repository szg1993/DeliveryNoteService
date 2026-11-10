using FluentAssertions;
using Common.Core.Model;
using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public class DbSetInfoTests
{
    private class TestEntity : IModel
    {
        public int Id { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity>? Entities1 { get; set; }

        public DbSet<TestEntity>? Entities2 { get; set; }
    }

    [Fact]
    public void Get_ShouldReturnAllDbSets()
    {
        DbSetInfo.Get<TestDbContext>()
            .Select(dbSet => dbSet.Name)
            .Should().BeEquivalentTo(
                nameof(TestDbContext.Entities1),
                nameof(TestDbContext.Entities2)
            );
    }
}