using Common.AspNetCore;
using Common.Core.Exceptions;
using Common.Core.Messaging;

namespace ApiTests;

public class RecipientListingTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ListRecipients_EmptyDatabaseReturnsEmptyPage()
    {
        var result = await Api.Recipients().GetAsync();

        result.Content.Should().BeEmpty();
        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(10);
        result.Paging.TotalCount.Should().Be(0);
        result.Paging.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task ListRecipients_ImmediatelyIncludesCreatedDetails()
    {
        var created = await Api.Recipients().PostAsync(RecipientRequests.Valid() with { Code = " acme ", Country = " us " });

        var result = await Api.Recipients().GetAsync();

        result.Content.Should().BeEquivalentTo([created]);
        result.Paging.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ListRecipients_RejectedCreationDoesNotChangeList()
    {
        var created = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        await Api.Recipients().PostRawAsync(RecipientRequests.Valid(code: "   "))
            .AssertBadRequestAsync("Recipient.InvalidCode");

        var result = await Api.Recipients().GetAsync();
        result.Content.Should().BeEquivalentTo([created]);
    }

    [Fact]
    public async Task ListRecipients_DefaultsToFirstTenInIdOrder()
    {
        for (var number = 1; number <= 11; number++)
            await Api.Recipients().PostAsync(RecipientRequests.Valid($"R{number}", $"Recipient {12 - number}"));

        var result = await Api.Recipients().GetAsync();

        result.Content.Select(recipient => recipient.Code).Should().Equal("R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10");
        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(10);
        result.Paging.TotalCount.Should().Be(11);
        result.Paging.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task ListRecipients_ReturnsRequestedPage()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid("FIRST"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("SECOND"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("THIRD"));

        var result = await Api.Recipients().GetAsync(QS().Page(2, 1));

        result.Content.Select(recipient => recipient.Code).Should().Equal("SECOND");
        result.Paging.Page.Should().Be(2);
        result.Paging.Limit.Should().Be(1);
        result.Paging.TotalCount.Should().Be(3);
        result.Paging.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ListRecipients_LimitZeroReturnsAllRecipients()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid("FIRST"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("SECOND"));

        var result = await Api.Recipients().GetAsync(QS().Page(1, 0));

        result.Content.Select(recipient => recipient.Code).Should().Equal("FIRST", "SECOND");
        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(0);
        result.Paging.TotalCount.Should().Be(2);
        result.Paging.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ListRecipients_CanSortByIdDescending()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid("FIRST"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("SECOND"));

        var result = await Api.Recipients().GetAsync(QS().Page(1, 0).Sort("Id", SortDirection.Desc));

        result.Content.Select(recipient => recipient.Code).Should().Equal("SECOND", "FIRST");
    }

    [Theory]
    [InlineData(SortDirection.Asc, "ALPHA1", "ALPHA2", "BETA")]
    [InlineData(SortDirection.Desc, "BETA", "ALPHA1", "ALPHA2")]
    public async Task ListRecipients_NameSortingUsesIdForTies(
        SortDirection direction,
        string first,
        string second,
        string third
    )
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid("BETA", "Beta"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("ALPHA1", "Alpha"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("ALPHA2", "Alpha"));

        var result = await Api.Recipients().GetAsync(QS().Page(1, 0).Sort("Name", direction));

        result.Content.Select(recipient => recipient.Code).Should().Equal(first, second, third);
    }

    [Theory]
    [InlineData("?Page=0")]
    [InlineData("?Page=-1")]
    [InlineData("?Limit=-1")]
    [InlineData("?Page=0&Limit=0")]
    public async Task ListRecipients_RejectsInvalidPagingEvenWhenEmpty(string query)
    {
        await Api.Recipients().GetRawAsync(new QueryString(query))
            .AssertBadRequestAsync(CommonErrorCodes.InvalidPageRequest);
    }

    [Theory]
    [InlineData("?SortBy=99")]
    [InlineData("?SortBy=-1")]
    [InlineData("?SortDir=99")]
    [InlineData("?SortDir=-1")]
    public async Task ListRecipients_RejectsUndefinedSortValues(string query)
    {
        await Api.Recipients().GetRawAsync(new QueryString(query))
            .AssertBadRequestAsync(CommonErrorCodes.InvalidSortField);
    }

    [Theory]
    [InlineData("?Page=abc")]
    [InlineData("?Page=2147483648")]
    [InlineData("?Limit=abc")]
    [InlineData("?Limit=2147483648")]
    [InlineData("?SortBy=Country")]
    [InlineData("?SortDir=Sideways")]
    public async Task ListRecipients_RejectsMalformedQueryValues(string query)
    {
        await Api.Recipients().GetRawAsync(new QueryString(query))
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
    }

    [Fact]
    public async Task ListRecipients_PageBeyondLastReturnsEmptyContentWithTotalCount()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid());

        var result = await Api.Recipients().GetAsync(QS().Page(2, 1));

        result.Content.Should().BeEmpty();
        result.Paging.TotalCount.Should().Be(1);
        result.Paging.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ListRecipients_LargePageDoesNotWrapOffset()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid());

        var result = await Api.Recipients().GetAsync(QS().Page(int.MaxValue, 2));

        result.Content.Should().BeEmpty();
        result.Paging.Page.Should().Be(int.MaxValue);
        result.Paging.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ListRecipients_DirectionWithoutFieldUsesIdOrder()
    {
        await Api.Recipients().PostAsync(RecipientRequests.Valid("FIRST"));
        await Api.Recipients().PostAsync(RecipientRequests.Valid("SECOND"));

        var result = await Api.Recipients().GetAsync(new QueryString("?SortDir=Desc"));

        result.Content.Select(recipient => recipient.Code).Should().Equal("SECOND", "FIRST");
    }
}