using Common.Core.Exceptions;
using Common.Core.Messaging;
using DeliveryNoteService.Application.UseCases.Recipients;
using System.Net;

namespace ApiTests;

[Trait("Database", "PostgreSQL")]
public class RecipientConcurrencyTests : PostgresDeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ConcurrentEquivalentCodes_CreateOneRecipientAndReturnOneConflict()
    {
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 8, 12, 30, 0, TimeSpan.Zero));
        var first = RecipientRequests.Valid(" acme-01 ", "First destination");
        var second = RecipientRequests.Valid("ACME-01", "Second destination");

        var responses = await Api.CreateRecipientsConcurrently(first, second);

        responses.Select(response => response.StatusCode).Should().BeEquivalentTo(
            new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }
        );
        using var createdResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Created);
        using var conflictResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Conflict);
        var created = (await createdResponse.ReadAsync<Envelope<RecipientDetailsDto>>()).Content;
        await conflictResponse.AssertErrorAsync(HttpStatusCode.Conflict)
            .AndErrorCodeAsync(CommonErrorCodes.UniqueKeyConflict);
        var listed = await Api.Recipients().GetAsync(new QueryString("?page=1&limit=0"));
        listed.Content.Should().ContainSingle().Which.Should().BeEquivalentTo(created);
        created.Code.Should().Be("ACME-01");
        created.Name.Should().BeOneOf("First destination", "Second destination");
    }
}