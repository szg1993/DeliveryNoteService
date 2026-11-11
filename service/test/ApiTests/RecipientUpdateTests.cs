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
}
