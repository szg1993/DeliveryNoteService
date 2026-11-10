using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace ApiTests;

public class NoteCreationTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task CreateNote_ReturnsCompleteNormalizedNoteWithServerFields()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        this.Time().SetDateTime(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        var request = NoteRequests.Valid(recipient.Id);

        using var response = await Api.Client.PostAsJsonAsync("/api/delivery-notes", request);

        await response.AssertStatusCode(HttpStatusCode.Created);
        var note = (await response.Content.ReadFromJsonAsync<JsonObject>())!["content"]!;
        note["id"]!.GetValue<int>().Should().BePositive();
        note["noteNbr"]!.GetValue<string>().Should().MatchRegex("^N[0-9]{5}$");
        note["noteStatusId"]!.GetValue<string>().Should().Be("Pending");
        note["createdDate"]!.GetValue<DateTimeOffset>().Should().Be(new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero));
        note["takeoverPlace"]!.GetValue<string>().Should().Be("Warehouse");
        note["contactInfo"]!["assignTo"]!.GetValue<string>().Should().Be("Driver");
        note["packageDetail"]!["pkgQty"]!.GetValue<decimal>().Should().Be(1.25m);
        note["deadline"]!["takeoverDate"]!.GetValue<string>().Should().Be("2020-01-01");
        note["noteLines"]![0]!["partDesc"]!.GetValue<string>().Should().Be("Part  A");
        note["recipient"]!["id"]!.GetValue<int>().Should().Be(recipient.Id);
    }

    [Fact]
    public async Task IdenticalRequests_CreateSeparateConsecutiveNumbers()
    {
        await this.SetNoteIdentity(0);
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);

        var first = await Api.Notes().PostAsync(request);
        var second = await Api.Notes().PostAsync(request);

        first.NoteNbr.Should().Be("N00001");
        second.NoteNbr.Should().Be("N00002");
        second.Id.Should().NotBe(first.Id);
        second.NoteLines.Single().Id.Should().BePositive();
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public async Task InvalidInput_ReturnsStandardBadRequest(string path, string? json, bool remove)
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        if (remove)
            NoteRequests.Remove(request, path);
        else
            NoteRequests.Set(request, path, json is null ? null : JsonNode.Parse(json));

        using var response = await Api.Notes().PostRawAsync(request);

        var error = await response.AssertErrorAsync(HttpStatusCode.BadRequest);
        error.ErrorCode.Should().BeOneOf("Note.InvalidInput", "error:DNC::InvalidHttpRequest");
    }

    public static IEnumerable<object?[]> InvalidInputs()
    {
        foreach (var path in new[] { "recipientId", "contactInfo", "packageDetail", "deadline", "noteLines" })
        {
            yield return new object?[] { path, null, false };
            yield return new object?[] { path, null, true };
        }
        yield return new object?[] { "recipientId", "0", false };
        yield return new object?[] { "recipientId", "-1", false };
        yield return new object?[] { "noteLines", "[]", false };
        yield return new object?[] { "noteLines.0", null, false };
        foreach (var path in new[] { "packageDetail.tareWgt", "packageDetail.pkgQty", "packageDetail.pkgSizeX", "packageDetail.pkgSizeY", "packageDetail.pkgSizeZ", "noteLines.0.partQty", "noteLines.0.partWgt" })
        {
            yield return new object?[] { path, null, true };
            yield return new object?[] { path, null, false };
            yield return new object?[] { path, "-0.01", false };
        }
        yield return new object?[] { "packageDetail.pkgQty", "0", false };
        yield return new object?[] { "noteLines.0.partQty", "0", false };
        foreach (var path in new[] { "deadline.takeoverDate", "deadline.shipDate", "deadline.estimatedArrivalDate" })
        {
            yield return new object?[] { path, null, true };
            yield return new object?[] { path, null, false };
            foreach (var value in new[] { System.Text.Json.JsonSerializer.Serialize(""), System.Text.Json.JsonSerializer.Serialize("2020-02-30"), System.Text.Json.JsonSerializer.Serialize("2020-1-01"), System.Text.Json.JsonSerializer.Serialize("2020-01-01T00:00:00Z"), "123" })
                yield return new object?[] { path, value, false };
        }
        yield return new object?[] { "deadline.takeoverDate", System.Text.Json.JsonSerializer.Serialize("2020-01-02"), false };
        yield return new object?[] { "deadline.estimatedArrivalDate", System.Text.Json.JsonSerializer.Serialize("2019-12-31"), false };
        foreach (var prefix in new[] { "", "contactInfo.", "packageDetail.", "deadline.", "noteLines.0." })
        {
            yield return new object?[] { prefix + "id", "1", false };
            yield return new object?[] { prefix + "unexpected", "true", false };
        }
        foreach (var field in new[] { "noteNbr", "noteStatusId", "createdDate", "userId" })
            yield return new object?[] { field, "1", false };
        yield return new object?[] { "contactInfo.assignTo", System.Text.Json.JsonSerializer.Serialize(new string('x', 201)), false };
        yield return new object?[] { "contactInfo.assignTo", "\"   \"", false };
        yield return new object?[] { "contactInfo.assignTo", null, true };
        yield return new object?[] { "contactInfo.assignTo", null, false };
        yield return new object?[] { "contactInfo.assignToPhone", System.Text.Json.JsonSerializer.Serialize(new string('x', 31)), false };
        yield return new object?[] { "contactInfo.assignToPhone", "\"   \"", false };
        yield return new object?[] { "contactInfo.assignToPhone", null, true };
        yield return new object?[] { "contactInfo.assignToPhone", null, false };
        yield return new object?[] { "contactInfo.contact", System.Text.Json.JsonSerializer.Serialize(new string('x', 201)), false };
        yield return new object?[] { "contactInfo.contact", "\"   \"", false };
        yield return new object?[] { "contactInfo.contact", null, true };
        yield return new object?[] { "contactInfo.contact", null, false };
        yield return new object?[] { "contactInfo.contactPhone", System.Text.Json.JsonSerializer.Serialize(new string('x', 31)), false };
        yield return new object?[] { "contactInfo.contactPhone", "\"   \"", false };
        yield return new object?[] { "contactInfo.contactPhone", null, true };
        yield return new object?[] { "contactInfo.contactPhone", null, false };
        yield return new object?[] { "packageDetail.tareWgtUm", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "packageDetail.tareWgtUm", "\"   \"", false };
        yield return new object?[] { "packageDetail.tareWgtUm", null, true };
        yield return new object?[] { "packageDetail.tareWgtUm", null, false };
        yield return new object?[] { "packageDetail.pkgScale", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "packageDetail.pkgScale", "\"   \"", false };
        yield return new object?[] { "packageDetail.pkgScale", null, true };
        yield return new object?[] { "packageDetail.pkgScale", null, false };
        yield return new object?[] { "packageDetail.pkgSizeUm", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "packageDetail.pkgSizeUm", "\"   \"", false };
        yield return new object?[] { "packageDetail.pkgSizeUm", null, true };
        yield return new object?[] { "packageDetail.pkgSizeUm", null, false };
        yield return new object?[] { "noteLines.0.partCode", System.Text.Json.JsonSerializer.Serialize(new string('x', 101)), false };
        yield return new object?[] { "noteLines.0.partCode", "\"   \"", false };
        yield return new object?[] { "noteLines.0.partCode", null, true };
        yield return new object?[] { "noteLines.0.partCode", null, false };
        yield return new object?[] { "noteLines.0.partDesc", System.Text.Json.JsonSerializer.Serialize(new string('x', 201)), false };
        yield return new object?[] { "noteLines.0.partDesc", "\"   \"", false };
        yield return new object?[] { "noteLines.0.partDesc", null, true };
        yield return new object?[] { "noteLines.0.partDesc", null, false };
        yield return new object?[] { "noteLines.0.partCmt", System.Text.Json.JsonSerializer.Serialize(new string('x', 201)), false };
        yield return new object?[] { "noteLines.0.partQtyUm", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "noteLines.0.partQtyUm", "\"   \"", false };
        yield return new object?[] { "noteLines.0.partQtyUm", null, true };
        yield return new object?[] { "noteLines.0.partQtyUm", null, false };
        yield return new object?[] { "noteLines.0.partWgtUm", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "noteLines.0.partWgtUm", "\"   \"", false };
        yield return new object?[] { "noteLines.0.partWgtUm", null, true };
        yield return new object?[] { "noteLines.0.partWgtUm", null, false };
        yield return new object?[] { "ekaerNbr", System.Text.Json.JsonSerializer.Serialize(new string('x', 11)), false };
        yield return new object?[] { "takeoverPlace", System.Text.Json.JsonSerializer.Serialize(new string('x', 31)), false };
        yield return new object?[] { "takeoverPlace", "\"   \"", false };
        yield return new object?[] { "takeoverPlace", null, true };
        yield return new object?[] { "takeoverPlace", null, false };
        yield return new object?[] { "category", System.Text.Json.JsonSerializer.Serialize(new string('x', 21)), false };
        yield return new object?[] { "category", "\"   \"", false };
        yield return new object?[] { "category", null, true };
        yield return new object?[] { "category", null, false };
        yield return new object?[] { "comments", System.Text.Json.JsonSerializer.Serialize(new string('x', 201)), false };

    }

    [Fact]
    public async Task UnknownRecipient_ReturnsNotFound()
    {
        using var response = await Api.Notes().PostRawAsync(NoteRequests.Valid(int.MaxValue));

        var error = await response.AssertErrorAsync(HttpStatusCode.NotFound);
        error.ErrorCode.Should().Be("Recipient.NotFound");
    }

    [Fact]
    public async Task Creation_PreservesAllDetailsAndDistinctOrderedLines()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        request["ekaerNbr"] = "   ";
        request["comments"] = " comment  text ";
        request["isUrgent"] = true;
        request["isCrackable"] = true;
        request["isDangerous"] = true;
        request["isOwnCost"] = true;
        request["noteLines"]![0]!["partCmt"] = " ";
        var secondLine = request["noteLines"]![0]!.DeepClone();
        secondLine["partDesc"] = " Second ";
        secondLine["partQty"] = 3.1234567890123456789012345678m;
        request["noteLines"]!.AsArray().Add(secondLine);

        var note = await Api.Notes().PostAsync(request);

        note.EkaerNbr.Should().BeNull();
        note.Comments.Should().Be("comment  text");
        note.Category.Should().Be("Goods");
        note.IsUrgent.Should().BeTrue();
        note.IsCrackable.Should().BeTrue();
        note.IsDangerous.Should().BeTrue();
        note.IsOwnCost.Should().BeTrue();
        note.ContactInfo.AssignToPhone.Should().Be("123");
        note.ContactInfo.Contact.Should().Be("Receiver");
        note.ContactInfo.ContactPhone.Should().Be("456");
        note.PackageDetail.TareWgt.Should().Be(0);
        note.PackageDetail.TareWgtUm.Should().Be("kg");
        note.PackageDetail.PkgScale.Should().Be("pallet");
        note.PackageDetail.PkgSizeX.Should().Be(0);
        note.PackageDetail.PkgSizeY.Should().Be(2.5m);
        note.PackageDetail.PkgSizeZ.Should().Be(3);
        note.PackageDetail.PkgSizeUm.Should().Be("m");
        note.NoteLines.Select(line => line.PartDesc).Should().Equal("Part  A", "Second");
        note.NoteLines.Select(line => line.PartCode).Should().Equal("A", "A");
        note.NoteLines[0].PartCmt.Should().BeNull();
        note.NoteLines[0].PartQtyUm.Should().Be("pcs");
        note.NoteLines[0].PartWgt.Should().Be(0);
        note.NoteLines[0].PartWgtUm.Should().Be("kg");
        note.NoteLines[1].PartQty.Should().Be(3.1234567890123456789012345678m);
        note.Deadline.ShipDate.Should().Be(new DateOnly(2020, 1, 1));
        note.Deadline.EstimatedArrivalDate.Should().Be(new DateOnly(2020, 1, 2));
        note.Recipient.Should().BeEquivalentTo(recipient);
        new[] { note.ContactInfo.Id, note.PackageDetail.Id, note.Deadline.Id }.Should().OnlyContain(id => id > 0);
        note.NoteLines.Select(line => line.Id).Should().OnlyHaveUniqueItems().And.OnlyContain(id => id > 0);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    public async Task MalformedBody_ReturnsStandardRequestError(string json)
    {
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var response = await Api.Client.PostAsync("/api/delivery-notes", content);

        var error = await response.AssertErrorAsync(HttpStatusCode.BadRequest);
        error.ErrorCode.Should().Be("error:DNC::InvalidHttpRequest");
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

        note.TakeoverPlace.Should().Be("Warehouse");
        note.IsUrgent.Should().BeFalse();
        note.IsCrackable.Should().BeFalse();
        note.IsDangerous.Should().BeFalse();
        note.IsOwnCost.Should().BeFalse();
        note.Deadline.EstimatedArrivalDate.Should().Be(new DateOnly(2020, 1, 1));
    }

    [Theory]
    [MemberData(nameof(TextLimits))]
    public async Task TextAtMaximumLength_IsTrimmedBeforeValidation(string path, int length)
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        NoteRequests.Set(request, path, JsonValue.Create("  " + new string('x', length) + "  "));

        using var response = await Api.Notes().PostRawAsync(request);

        await response.AssertStatusCode(HttpStatusCode.Created);
        var note = (await response.Content.ReadFromJsonAsync<JsonObject>())!["content"]!;
        NoteRequests.Get(note, path)!.GetValue<string>().Should().Be(new string('x', length));
    }

    public static IEnumerable<object[]> TextLimits()
    {
        yield return new object[] { "contactInfo.assignTo", 200 };
        yield return new object[] { "contactInfo.assignToPhone", 30 };
        yield return new object[] { "contactInfo.contact", 200 };
        yield return new object[] { "contactInfo.contactPhone", 30 };
        yield return new object[] { "packageDetail.tareWgtUm", 10 };
        yield return new object[] { "packageDetail.pkgScale", 10 };
        yield return new object[] { "packageDetail.pkgSizeUm", 10 };
        yield return new object[] { "noteLines.0.partCode", 100 };
        yield return new object[] { "noteLines.0.partDesc", 200 };
        yield return new object[] { "noteLines.0.partCmt", 200 };
        yield return new object[] { "noteLines.0.partQtyUm", 10 };
        yield return new object[] { "noteLines.0.partWgtUm", 10 };
        yield return new object[] { "ekaerNbr", 10 };
        yield return new object[] { "takeoverPlace", 30 };
        yield return new object[] { "category", 20 };
        yield return new object[] { "comments", 200 };
    }

    [Fact]
    public async Task ExhaustedNumber_ReturnsSpecificBusinessFailure()
    {
        await this.SetNoteIdentity(99999);
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        using var response = await Api.Notes().PostRawAsync(NoteRequests.Valid(recipient.Id));

        var error = await response.AssertErrorAsync(HttpStatusCode.BadRequest);
        error.ErrorCode.Should().Be("Note.NumberExhausted");
    }
}