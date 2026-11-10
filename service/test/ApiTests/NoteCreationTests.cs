using Common.AspNetCore;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

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
    public async Task TimestampDeadline_ReturnsStandardRequestError()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        request["deadline"]!["takeoverDate"] = "2020-01-01T00:00:00Z";

        using var response = await Api.Notes().PostRawAsync(request);

        await response.AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
    }

    [Fact]
    public async Task OversizedText_ReturnsExistingValidationError()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        request["comments"] = new string('x', 201);

        using var response = await Api.Notes().PostRawAsync(request);

        await response.AssertBadRequestAsync("Note.InvalidInput");
    }

    [Fact]
    public async Task KnownPropertyNames_AreCaseInsensitiveAndOmittedFlagsAreFalse()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        request["TAKEOVERPLACE"] = request["takeoverPlace"]!.DeepClone();
        request.Remove("takeoverPlace");
        request["deadline"]!["estimatedArrivalDate"] = "2020-01-01";

        var note = await Api.Notes().PostAsync(request);

        note.TakeoverPlace.Should().Be(" Warehouse ");
        note.IsUrgent.Should().BeFalse();
        note.IsCrackable.Should().BeFalse();
        note.IsDangerous.Should().BeFalse();
        note.IsOwnCost.Should().BeFalse();
        note.Deadline.EstimatedArrivalDate.Should().Be(new DateOnly(2020, 1, 1));
    }
}