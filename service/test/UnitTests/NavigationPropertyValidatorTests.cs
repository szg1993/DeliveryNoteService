using Common.Core.Model;
using Common.Core.Persistence;
using Common.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;
using UnitTests.TestSuite;

namespace UnitTests;

[Collection(SqlUnitTestCollection.Name)]
public class NavigationPropertyValidatorTests : SqlUnitTestBase
{
    private class TestParent : IModel
    {
        public int Id { get; set; }

        public TestModel1? Child1 { get; set; } = null;
        public TestModel2 Child2 { get; set; } = null!;
        public ICollection<TestModel3> Children { get; set; } = null!;
    }

    private class TestModel1 : IModel { public int Id { get; set; } }

    private class TestModel2 : IModel { public int Id { get; set; } }

    private class TestModel3 : IModel { public int Id { get; set; } }

    private class TestDbContext(NewEntityLogger logger, DbContextOptions options)
        : AppDbContextBase(logger, options)
    {
        public DbSet<TestParent> Parents => Set<TestParent>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsTracked(bool usePostgres)
    {
        if (usePostgres)
            await InitPostgresAsync<TestDbContext>();
        else
            InitSQLite<TestDbContext>();
        await InitDataAsync<TestDbContext>(AddTestData);
        using var db = CreateDbContext<TestDbContext>();
        var parent = await db.Parents.SingleAsync();
        var validator = new NavigationPropertyValidator(db);

        validator.IsTracked(parent).Should().Be(true);
        var newEntity = new TestParent() { Child2 = new TestModel2() };
        validator.IsTracked(newEntity).Should().Be(false);
        db.Parents.Add(newEntity);
        validator.IsTracked(newEntity).Should().Be(false);
        await db.SaveChangesAsync();
        validator.IsTracked(newEntity).Should().Be(true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task IsLoaded(bool usePostgres, bool include)
    {
        if (usePostgres)
            await InitPostgresAsync<TestDbContext>();
        else
            InitSQLite<TestDbContext>();

        await TestIsLoaded(include);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void ValidateLoaded(bool isParentTracked, bool isLoaded, bool shouldThrow)
    {
        var parent = new TestParent();
        var validatorMock = CreateValidatorMock();
        SetupIsParentTracked(validatorMock, parent, isParentTracked);
        SetupValidate(validatorMock, parent, isLoaded);

        if (shouldThrow)
        {
            Assert.Throws<InvalidOperationException>(() =>
                NavigationPropertyValidatorExtensions.ValidateLoaded(
                    validatorMock.Object, parent, p => p.Child2
                )
            ).Message.Should().Be("TestParent.Child2 must be loaded");
        }
        else
        {
            NavigationPropertyValidatorExtensions.ValidateLoaded(
                 validatorMock.Object, parent, p => p.Child2
            ).Should().BeSameAs(parent.Child2);
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void ValidateCollectionLoaded(bool isParentTracked, bool isLoaded, bool shouldThrow)
    {
        var parent = new TestParent();
        var validatorMock = CreateValidatorMock();
        SetupIsParentTracked(validatorMock, parent, isParentTracked);
        SetupValidateCollection(validatorMock, parent, isLoaded);

        if (shouldThrow)
        {
            Assert.Throws<InvalidOperationException>(() =>
                NavigationPropertyValidatorExtensions.ValidateCollectionLoaded(
                    validatorMock.Object, parent, p => p.Children
                )
            ).Message.Should().Be("TestParent.Children must be loaded");
        }
        else
        {
            NavigationPropertyValidatorExtensions.ValidateCollectionLoaded(
                 validatorMock.Object, parent, p => p.Children
            ).Should().BeSameAs(parent.Children);
        }
    }

    private static void AddTestData(TestDbContext db)
    {
        db.Parents.Add(new TestParent()
        {
            Child1 = new TestModel1() { Id = 10 },
            Child2 = new TestModel2() { Id = 20 },
            Children = [new TestModel3() { Id = 31 }, new TestModel3() { Id = 32 }]
        });
    }

    private async Task TestIsLoaded(bool include)
    {
        await InitDataAsync<TestDbContext>(AddTestData);
        using var db = CreateDbContext<TestDbContext>();
        var q = db.Parents.AsQueryable();
        if (include)
        {
            q = q
            .Include(p => p.Child1)
            .Include(p => p.Child2)
            .Include(p => p.Children);
        }
        var parent = await q.SingleAsync();
        var validator = new NavigationPropertyValidator(db);

        validator.IsLoaded(parent, p => p.Child1).Should().Be(include);
        validator.IsLoaded(parent, p => p.Child2).Should().Be(include);
        validator.IsCollectionLoaded(parent, p => p.Children).Should().Be(include);
    }

    private static void SetupIsParentTracked(Mock<INavigationPropertyValidator> validatorMock, TestParent parent, bool isParentTracked)
    {
        validatorMock
            .Setup(v => v.IsTracked(parent))
            .Returns(isParentTracked);
    }

    private static void SetupValidateCollection(Mock<INavigationPropertyValidator> validatorMock, TestParent parent, bool isLoaded)
    {
        validatorMock
            .Setup(v => v.IsCollectionLoaded(
                parent,
                It.IsAny<Expression<Func<TestParent, IEnumerable<TestModel3>>>>()
            )).Returns(isLoaded);
    }

    private static void SetupValidate(Mock<INavigationPropertyValidator> validatorMock, TestParent parent, bool isLoaded)
    {
        validatorMock
            .Setup(v => v.IsLoaded(
                parent,
                It.IsAny<Expression<Func<TestParent, TestModel2?>>>()
            )).Returns(isLoaded);
    }

    private static Mock<INavigationPropertyValidator> CreateValidatorMock()
    {
        return new Mock<INavigationPropertyValidator>(MockBehavior.Strict);
    }
}