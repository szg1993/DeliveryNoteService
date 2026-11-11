using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
using System.Net;

namespace ApiTests;

public class RecipientUpdateTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime()
    {
        var createdAt = new DateTimeOffset(2026, 11, 8, 12, 30, 0, TimeSpan.Zero);
        this.Time().SetDateTime(createdAt);
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        this.Time().SetDateTime(createdAt.AddDays(1));
        var request = RecipientRequests.ValidUpdate() with
        {
            Name = " Globex  Kft ",
            Country = " us ",
            Zip = " 02134 ",
            City = " Boston ",
            Address = " Elm street 5 "
        };

        var response = await Api.Recipients(original.Id).PutRawAsync(request);

        await response.AssertStatusCode(HttpStatusCode.OK);
        var expected = new RecipientDetailsDto
        {
            Id = original.Id,
            Code = "ACME",
            Name = " Globex  Kft ",
            Country = "US",
            Zip = " 02134 ",
            City = " Boston ",
            Address = " Elm street 5 ",
            CreatedAt = createdAt,
            IsForeign = true
        };
        (await response.ReadAsync<Envelope<RecipientDetailsDto>>()).Content.Should().BeEquivalentTo(expected);
        (await Api.Recipients(original.Id).GetAsync()).Should().BeEquivalentTo(expected);
        (await Api.Recipients().GetAsync()).Content.Should().BeEquivalentTo([expected]);
    }

    [Fact]
    public async Task UpdateRecipient_IsShownOnExistingDeliveryNote()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var note = await Api.Notes().PostAsync(NoteRequests.Valid(recipient.Id));

        var updated = await Api.Recipients(recipient.Id).PutAsync(RecipientRequests.ValidUpdate());

        (await Api.Notes(note.Id).GetAsync()).Recipient.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task UpdateRecipient_IgnoresSuppliedCode()
    {
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid(code: "ACME"));
        var request = new UpdateWithCodeDto
        {
            Name = "Globex Kft",
            Country = "HU",
            Zip = "6720",
            City = "Szeged",
            Address = "Kossuth street 2",
            Code = "GLOBEX"
        };

        var updated = await Api.Recipients(original.Id).PutAsync(request);

        updated.Should().BeEquivalentTo(original with { Name = "Globex Kft", Zip = "6720", City = "Szeged", Address = "Kossuth street 2" });
    }

    [Fact]
    public async Task UpdateRecipient_RejectsInvalidDetailsWithoutChangingRecipient()
    {
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = RecipientRequests.ValidUpdate() with { Country = "ZZ" };

        var error = await Api.Recipients(original.Id).PutRawAsync(request)
            .AssertErrorAsync(HttpStatusCode.BadRequest);

        error.ErrorCode.Should().Be("Recipient.InvalidCountry");
        (await Api.Recipients(original.Id).GetAsync()).Should().BeEquivalentTo(original);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(123456)]
    public Task UpdateRecipient_UnknownId_ReturnsNotFound(int id)
        => Api.Recipients(id).PutRawAsync(RecipientRequests.ValidUpdate())
            .AssertNotFoundAsync("Recipient.NotFound");

    private sealed record UpdateWithCodeDto : UpdateRecipientDto
    {
        public string Code { get; init; } = null!;
    }
}