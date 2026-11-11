using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
using System.Net;

namespace ApiTests;

[Trait("Database", "PostgreSQL")]
public class RecipientConcurrencyTests : PostgresDeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ConcurrentEquivalentCodes_CreateOneRecipientAndRejectOneDuplicate()
    {
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 8, 12, 30, 0, TimeSpan.Zero));
        var first = RecipientRequests.Valid(" acme-01 ", "First destination");
        var second = RecipientRequests.Valid("ACME-01", "Second destination");

        var responses = await Api.CreateRecipientsConcurrently(first, second);

        responses.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.Created);
        using var createdResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Created);
        using var rejectedResponse = responses.Single(response => response.StatusCode != HttpStatusCode.Created);
        var created = (await createdResponse.ReadAsync<Envelope<RecipientDetailsDto>>()).Content;
        var error = await rejectedResponse.AssertErrorAsync(HttpStatusCode.BadRequest);
        error.ErrorCode.Should().Be("Recipient.AlreadyExists");
        var listed = await Api.Recipients().GetAsync(new QueryString("?page=1&limit=0"));
        listed.Content.Should().ContainSingle().Which.Should().BeEquivalentTo(created);
        created.Code.Should().Be("ACME-01");
        created.Name.Should().BeOneOf("First destination", "Second destination");
    }
}