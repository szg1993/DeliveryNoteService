using FluentAssertions;
using Medusa.Enap.Common.Core.Utils;
using System.Linq.Expressions;

namespace UnitTests.Core.Utils;

public class ExpressionUtilsTest
{
    private readonly Expression<Func<int, bool>> _isEven = i => i % 2 == 0;
    readonly Expression<Func<int, bool>> _isDivisibleBy3 = i => i % 3 == 0;

    [Fact]
    public void NotTest()
    {
        int[] input = [1, 2, 3, 4, 5, 6];

        input.AsQueryable()
            .Where(ExpressionUtils.Not(_isEven))
            .Should().BeEquivalentTo([1, 3, 5]);
    }

    [Fact]
    public void AndTest()
    {
        int[] input = [1, 2, 3, 6, 9, 22, 24];

        input.AsQueryable()
            .Where(_isEven.And(_isDivisibleBy3))
            .Should().BeEquivalentTo([6, 24]);
    }

    [Fact]
    public void OrTest()
    {
        int[] input = [1, 2, 3, 4, 5, 6, 9];

        input.AsQueryable()
            .Where(_isEven.Or(_isDivisibleBy3))
            .Should().BeEquivalentTo([2, 3, 4, 6, 9]);
    }

    [Fact]
    public void ReplaceParamTest()
    {
        ParentClass[] input = [new(1), new(2), new(3), new(4), new(5), new(6)];

        input.AsQueryable()
            .Where(_isEven.ReplaceParam((ParentClass parent) => parent.Num))
            .Select(p => p.Num)
            .Should().BeEquivalentTo([2, 4, 6]);
    }

    private record ParentClass(int Num);
}
