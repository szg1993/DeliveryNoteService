using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using System.Net;
using System.Text.Json.Nodes;

namespace ApiTests;

public class NoteDecimalTests : DeliveryNoteServiceApiTestBase
{
    [Theory]
    [InlineData("packageDetail.tareWgt", "1e-28", "0.0000000000000000000000000001")]
    [InlineData("noteLines.0.partQty", "1.2300e2", "123")]
    public async Task ExactlyRepresentableNumber_IsSavedWithoutChangingItsValue(
        string path,
        string number,
        string expected
    )
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        using var response = await Api.PostRawNumberAsync(recipient.Id, path, number);

        await response.AssertStatusCode(HttpStatusCode.Created);
        var created = (await response.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        using var savedResponse = await Api.Notes(created.Id).GetRawAsync();
        await savedResponse.AssertStatusCode(HttpStatusCode.OK);
        var saved = (await savedResponse.ReadAsync<JsonObject>())["content"]!;
        NoteRequests.Get(saved, path)!.GetValue<decimal>().Should().Be(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture));
    }
}