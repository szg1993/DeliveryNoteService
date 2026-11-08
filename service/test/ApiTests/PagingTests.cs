using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Messaging;

namespace ApiTests;

public class PagingTests : DemoServiceApiTestBase
{
    [Fact]
    public async Task BasicPaging()
    {
        await Given.DemoModels(7);

        var result = await Api.DemoModels().GetAsync(QS().Page(2, 3));

        result.Paging.Page.Should().Be(2);
        result.Paging.Limit.Should().Be(3);
        result.Paging.TotalCount.Should().Be(7);
        result.Paging.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task DefaultValues()
    {
        await Given.DemoModels(11);

        var result = await Api.DemoModels().GetAsync();

        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(10);
        result.Paging.TotalCount.Should().Be(11);
        result.Paging.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Unlimited()
    {
        await Given.DemoModels(11);

        var result = await Api.DemoModels().GetAsync(QS().Unpaged());

        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(0);
        result.Paging.TotalCount.Should().Be(11);
        result.Paging.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task InvalidLimit()
    {
        await Given.DemoModels(11);

        await Api.DemoModels().GetRawAsync(QS().Page(1, -1))
            .AssertBadRequestAsync(CommonErrorCodes.InvalidPageRequest);
    }

    [Fact]
    public async Task InvalidPage()
    {
        await Given.DemoModels(11);

        await Api.DemoModels().GetRawAsync(QS().Page(0, 1))
            .AssertBadRequestAsync(CommonErrorCodes.InvalidPageRequest);
    }

    [Fact]
    public async Task BasicSorting()
    {
        await Given.DemoModels(4);

        var result = await Api.DemoModels().GetAsync(QS().Unpaged().Sort("Id", SortDirection.Desc));

        result.Content
            .Should().BeInDescendingOrder(m => m.Id);
    }
}
