using Common.AspNetCore;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
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
        result.Should().BeEquivalentTo(new RecipientDetailsDto
        {
            Id = created.Content.Id,
            Code = "ACME",
            Name = "Acme Ltd",
            Country = "HU",
            Zip = "0101",
            City = "Budapest",
            Address = "Main street 1",
            CreatedAt = createdAt,
            IsForeign = false
        });
        result.Id.Should().BePositive();
        created.Content.Should().BeEquivalentTo(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public Task NonPositiveRecipientId_ReturnsNotFound(int id)
        => Api.Recipients(id).GetRawAsync()
            .AssertNotFoundAsync("Recipient.NotFound");

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("2147483648")]
    public Task MalformedRecipientId_ReturnsBadRequest(string id)
        => Api.Client.GetAsync($"/api/recipients/{id}")
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);

    [Fact]
    public Task UnknownRecipientId_ReturnsNotFound()
        => Api.Recipients(123456).GetRawAsync()
            .AssertNotFoundAsync("Recipient.NotFound");
}