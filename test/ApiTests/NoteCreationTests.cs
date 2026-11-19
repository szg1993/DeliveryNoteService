using Common.AspNetCore;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiTests;

public class NoteCreationTests : DeliveryNoteServiceApiTestBase
{
    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    public async Task MalformedBody_ReturnsStandardRequestError(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await Api.Client.PostAsync("/api/delivery-notes", content);

        await response.AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
    }

    [Fact]
    public async Task KnownPropertyNames_AreCaseInsensitiveAndOmittedFlagsAreFalse()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var defaults = NoteRequests.Valid(recipient.Id);
        var request = defaults with
        {
            Deadline = defaults.Deadline with { EstimatedArrivalDate = new DateOnly(2020, 1, 1) }
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = new UpperCaseNamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        using var response = await Api.Client.PostAsJsonAsync("/api/delivery-notes", request, options);

        var note = await Api.AssertSavedNote(response);
        note.TakeoverPlace.Should().Be(" Warehouse ");
        note.IsUrgent.Should().BeFalse();
        note.IsCrackable.Should().BeFalse();
        note.IsDangerous.Should().BeFalse();
        note.IsOwnCost.Should().BeFalse();
        note.Deadline.EstimatedArrivalDate.Should().Be(new DateOnly(2020, 1, 1));
    }

    private sealed class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToUpperInvariant();
    }
}