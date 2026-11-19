namespace ApiTests;

public class NoteDecimalTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task DecimalValues_AreSavedWithoutChangingTheirValue()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var defaults = NoteRequests.Valid(recipient.Id);
        var request = defaults with
        {
            PackageDetail = defaults.PackageDetail with { TareWgt = 0.0000000000000000000000000001m },
            NoteLines = [defaults.NoteLines[0] with { PartQty = 123m }]
        };

        using var response = await Api.Notes().PostRawAsync(request);

        var saved = await Api.AssertSavedNote(response);
        saved.PackageDetail.TareWgt.Should().Be(request.PackageDetail.TareWgt);
        saved.NoteLines[0].PartQty.Should().Be(request.NoteLines[0].PartQty);
    }
}