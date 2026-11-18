namespace ApiTests;

public class NoteListingTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ListNotes_IncludesCreatedNoteWithCurrentRecipient()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var created = await Api.Notes().PostAsync(NoteRequests.Valid(recipient.Id));
        var updatedRecipient = await Api.Recipients(recipient.Id).PutAsync(RecipientRequests.ValidUpdate());

        var result = await Api.Notes().GetAsync();

        result.Content.Should().BeEquivalentTo(
            [created with { Recipient = updatedRecipient }],
            options => options.WithStrictOrdering()
        );
        result.Paging.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ListNotes_WithoutNotesReturnsEmptyFirstPage()
    {
        var result = await Api.Notes().GetAsync();

        result.Content.Should().BeEmpty();
        result.Paging.Page.Should().Be(1);
        result.Paging.Limit.Should().Be(10);
        result.Paging.TotalCount.Should().Be(0);
        result.Paging.TotalPages.Should().Be(0);
    }
}