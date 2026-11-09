using Common.AspNetCore;
using Common.Core.Messaging;
using DeliveryNoteService.Application.UseCases.Recipients;
using System.Net;

namespace ApiTests;

public class RecipientDetailsTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task CreatedRecipient_CanBeRetrievedThroughLocation()
    {
        var createdAt = new DateTimeOffset(2026, 11, 8, 12, 30, 0, TimeSpan.Zero);
        this.Time().SetDateTime(createdAt);
        var createdResponse = await Api.Recipients().PostRawAsync(RecipientRequests.Valid());
        var created = await createdResponse.ReadAsync<Envelope<RecipientDetailsDto>>();

        var response = await Api.Client.GetAsync(createdResponse.Headers.Location);

        await response.AssertStatusCode(HttpStatusCode.OK);
        var result = (await response.ReadAsync<Envelope<RecipientDetailsDto>>()).Content;
        result.Should().BeEquivalentTo(new RecipientDetailsDto(
            created.Content.Id,
            "ACME",
            "Acme Ltd",
            "HU",
            "0101",
            "Budapest",
            "Main street 1",
            createdAt,
            false
        ));
        result.Id.Should().BePositive();
        created.Content.Should().BeEquivalentTo(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public Task InvalidRecipientId_ReturnsBadRequest(int id)
        => Api.Recipients(id).GetRawAsync()
            .AssertBadRequestAsync("InvalidRecipientId");

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("2147483648")]
    public Task MalformedRecipientId_ReturnsBadRequest(string id)
        => Api.Client.GetAsync($"/api/recipients/{id}")
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);

    [Fact]
    public Task UnknownRecipientId_ReturnsNotFound()
        => Api.Recipients(123456).GetRawAsync()
            .AssertNotFoundAsync("RecipientNotFound");
}