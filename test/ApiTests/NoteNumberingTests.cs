using System.Net;

namespace ApiTests;

public class NoteNumberingTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ConcurrentCreations_HaveDistinctNumbersAndCompleteSavedDetails()
    {
        await this.SetNoteIdentity(0);
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var firstRequest = NoteRequests.Valid(recipient.Id) with { Comments = "First concurrent note" };
        var secondRequest = NoteRequests.Valid(recipient.Id) with { Comments = "Second concurrent note" };

        var responses = await Api.CreateNotesConcurrently(firstRequest, secondRequest);

        using var firstResponse = responses[0];
        using var secondResponse = responses[1];
        var first = await Api.AssertSavedNote(firstResponse);
        var second = await Api.AssertSavedNote(secondResponse);
        first.Id.Should().NotBe(second.Id);
        new[] { first.NoteNbr, second.NoteNbr }.Should().BeEquivalentTo("N00001", "N00002");
        first.Comments.Should().Be("First concurrent note");
        second.Comments.Should().Be("Second concurrent note");
    }

    [Fact]
    public async Task FinalNumber_IsSavedAndRepeatedExhaustionLeavesItUnchanged()
    {
        await this.SetNoteIdentity(99998);
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id) with { Comments = "Last available number" };
        var created = await Api.Notes().PostAsync(request);
        var rejectedRequest = NoteRequests.Valid(recipient.Id) with { Comments = "Must not replace the last note" };

        using var firstRejection = await Api.Notes().PostRawAsync(rejectedRequest);
        using var secondRejection = await Api.Notes().PostRawAsync(rejectedRequest);

        created.Id.Should().Be(99999);
        created.NoteNbr.Should().Be("N99999");
        (await firstRejection.AssertErrorAsync(HttpStatusCode.BadRequest)).ErrorCode.Should().Be("Note.NumberExhausted");
        (await secondRejection.AssertErrorAsync(HttpStatusCode.BadRequest)).ErrorCode.Should().Be("Note.NumberExhausted");
        using var absent = await Api.Notes(100000).GetRawAsync();
        await absent.AssertNotFoundAsync("Note.NotFound");
        var saved = await Api.Notes(99999).GetAsync();
        saved.Should().BeEquivalentTo(created, options => options.WithStrictOrdering());
        saved.Comments.Should().Be("Last available number");
    }

    [Fact]
    public async Task ConcurrentCreationsForFinalNumber_SaveOneCompleteNoteAndRejectTheOther()
    {
        await this.SetNoteIdentity(99998);
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var firstRequest = NoteRequests.Valid(recipient.Id) with { Comments = "First contender" };
        var secondRequest = NoteRequests.Valid(recipient.Id) with { Comments = "Second contender" };

        var responses = await Api.CreateNotesConcurrently(firstRequest, secondRequest);

        using var firstResponse = responses[0];
        using var secondResponse = responses[1];
        responses.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.Created);
        responses.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.BadRequest);
        var success = responses.Single(response => response.StatusCode == HttpStatusCode.Created);
        var failure = responses.Single(response => response.StatusCode == HttpStatusCode.BadRequest);
        var created = await Api.AssertSavedNote(success);
        created.Id.Should().Be(99999);
        created.NoteNbr.Should().Be("N99999");
        created.Comments.Should().Be(success == firstResponse ? "First contender" : "Second contender");
        (await failure.AssertErrorAsync(HttpStatusCode.BadRequest)).ErrorCode.Should().Be("Note.NumberExhausted");
        using var absent = await Api.Notes(100000).GetRawAsync();
        await absent.AssertNotFoundAsync("Note.NotFound");
    }

    [Fact]
    public async Task SimilarlyNamedTechnicalConstraint_IsNotReportedAsNumberExhaustion()
    {
        await this.SetNoteIdentity(0);
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        await this.RejectNotesWithUnrelatedConstraint();

        using var response = await Api.Notes().PostRawAsync(NoteRequests.Valid(recipient.Id));

        var error = await response.AssertErrorAsync(HttpStatusCode.InternalServerError);
        error.ErrorCode.Should().NotBe("Note.NumberExhausted");
        using var absent = await Api.Notes(1).GetRawAsync();
        await absent.AssertNotFoundAsync("Note.NotFound");
    }
}