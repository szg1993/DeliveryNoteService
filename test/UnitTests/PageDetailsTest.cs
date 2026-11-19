using Common.Core.Messaging;
using FluentAssertions;

namespace UnitTests;

public class PageDetailsTest
{
    [Theory]
    [InlineData(1, 0, 3, 1)]
    [InlineData(1, 1, 3, 3)]
    [InlineData(1, 2, 3, 2)]
    [InlineData(1, 3, 3, 1)]
    [InlineData(1, 4, 3, 1)]
    public void TotalPagesCalculations(
        int page, int limit, int totalCount, int expectedTotalPages
    )
    {
        var pd = new PageDetails(
            new PageRequest(page, limit),
            totalCount: totalCount
        );

        pd.Should().BeEquivalentTo(
            new PageDetails
            {
                Page = page,
                Limit = limit,
                TotalCount = totalCount,
                TotalPages = expectedTotalPages
            }
        );
    }
}