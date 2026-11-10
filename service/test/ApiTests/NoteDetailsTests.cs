using Common.AspNetCore;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using System.Net;
using System.Text.Json.Nodes;

namespace ApiTests;

public class NoteDetailsTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task CreatedNote_CanBeRetrievedThroughLocation()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        using var createdResponse = await Api.Notes().PostRawAsync(NoteRequests.Valid(recipient.Id));
        await createdResponse.AssertStatusCode(HttpStatusCode.Created);
        var created = (await createdResponse.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        createdResponse.Headers.Location.Should().NotBeNull();

        using var response = await Api.Client.GetAsync(createdResponse.Headers.Location);

        await response.AssertStatusCode(HttpStatusCode.OK);
        var saved = (await response.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        saved.Should().BeEquivalentTo(created, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task RetrievedNote_PreservesCompleteGraphAndOrderedDistinctLines()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var createdAt = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);
        this.Time().SetDateTime(createdAt);
        var request = NoteRequests.Valid(recipient.Id);
        request["ekaerNbr"] = " EK123 ";
        request["comments"] = " Handle  carefully ";
        request["isUrgent"] = true;
        request["isCrackable"] = true;
        request["isDangerous"] = true;
        request["isOwnCost"] = true;
        NoteRequests.Set(request, "packageDetail.tareWgt", JsonValue.Create(0.1234567890123456789012345678m));
        NoteRequests.Set(request, "noteLines.0.partCmt", JsonValue.Create(" First  line "));
        request["noteLines"]!.AsArray().Add(JsonNode.Parse(
            """
            { "partCode": " A ", "partDesc": " Second part ", "partCmt": "  ", "partQty": 0.125, "partQtyUm": " box ", "partWgt": 3.75, "partWgtUm": " lb " }
            """
        ));
        var created = await Api.Notes().PostAsync(request);
        this.Time().SetDateTime(createdAt.AddDays(1));

        var saved = await Api.Notes(created.Id).GetAsync();

        saved.Should().BeEquivalentTo(created, options => options.WithStrictOrdering());
        saved.Should().BeEquivalentTo(
            new
            {
                EkaerNbr = "EK123",
                TakeoverPlace = "Warehouse",
                Category = "Goods",
                Comments = "Handle  carefully",
                IsUrgent = true,
                IsCrackable = true,
                IsDangerous = true,
                IsOwnCost = true,
                CreatedDate = createdAt,
                RecipientId = recipient.Id,
                Recipient = recipient,
                ContactInfo = new { AssignTo = "Driver", AssignToPhone = "123", Contact = "Receiver", ContactPhone = "456" },
                PackageDetail = new { TareWgt = 0.1234567890123456789012345678m, TareWgtUm = "kg", PkgQty = 1.25m, PkgScale = "pallet", PkgSizeX = 0m, PkgSizeY = 2.5m, PkgSizeZ = 3m, PkgSizeUm = "m" },
                Deadline = new { TakeoverDate = new DateOnly(2020, 1, 1), ShipDate = new DateOnly(2020, 1, 1), EstimatedArrivalDate = new DateOnly(2020, 1, 2) },
                NoteLines = new[]
                {
                    new { PartCode = "A", PartDesc = "Part  A", PartCmt = (string?)"First  line", PartQty = 2.25m, PartQtyUm = "pcs", PartWgt = 0m, PartWgtUm = "kg" },
                    new { PartCode = "A", PartDesc = "Second part", PartCmt = (string?)null, PartQty = 0.125m, PartQtyUm = "box", PartWgt = 3.75m, PartWgtUm = "lb" }
                }
            },
            options => options.WithStrictOrdering()
        );
        saved.ContactInfo.Id.Should().BePositive();
        saved.PackageDetail.Id.Should().BePositive();
        saved.Deadline.Id.Should().BePositive();
        saved.NoteLines.Select(line => line.Id).Should().OnlyHaveUniqueItems().And.OnlyContain(id => id > 0);
        saved.NoteStatusId.ToString().Should().Be("Pending");
    }

    [Fact]
    public async Task RetrievedNote_ReturnsExactCalendarDatesAndNormalizedOptionalText()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        request["ekaerNbr"] = "  ";
        request["comments"] = "  ";
        NoteRequests.Set(request, "noteLines.0.partCmt", JsonValue.Create("  "));
        var created = await Api.Notes().PostAsync(request);

        using var response = await Api.Notes(created.Id).GetRawAsync();

        await response.AssertStatusCode(HttpStatusCode.OK);
        var saved = (await response.ReadAsync<JsonObject>())["content"]!;
        saved["deadline"]!["takeoverDate"]!.GetValue<string>().Should().Be("2020-01-01");
        saved["deadline"]!["shipDate"]!.GetValue<string>().Should().Be("2020-01-01");
        saved["deadline"]!["estimatedArrivalDate"]!.GetValue<string>().Should().Be("2020-01-02");
        saved["ekaerNbr"].Should().BeNull();
        saved["comments"].Should().BeNull();
        saved["noteLines"]![0]!["partCmt"].Should().BeNull();
    }

    [Fact]
    public async Task IdenticalCreations_HaveSeparateRetrievableLocations()
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        using var firstResponse = await Api.Notes().PostRawAsync(request);
        using var secondResponse = await Api.Notes().PostRawAsync(request);
        await firstResponse.AssertStatusCode(HttpStatusCode.Created);
        await secondResponse.AssertStatusCode(HttpStatusCode.Created);
        var firstCreated = (await firstResponse.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        var secondCreated = (await secondResponse.ReadAsync<Envelope<NoteDetailsDto>>()).Content;

        using var firstRead = await Api.Client.GetAsync(firstResponse.Headers.Location);
        using var secondRead = await Api.Client.GetAsync(secondResponse.Headers.Location);

        await firstRead.AssertStatusCode(HttpStatusCode.OK);
        await secondRead.AssertStatusCode(HttpStatusCode.OK);
        var first = (await firstRead.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        var second = (await secondRead.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        firstResponse.Headers.Location.Should().NotBe(secondResponse.Headers.Location);
        first.Id.Should().NotBe(second.Id);
        first.NoteNbr.Should().NotBe(second.NoteNbr);
        first.Should().BeEquivalentTo(firstCreated, options => options.WithStrictOrdering());
        second.Should().BeEquivalentTo(secondCreated, options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(123456)]
    public Task AbsentNote_ReturnsStandardNotFound(int id)
        => Api.Notes(id).GetRawAsync().AssertNotFoundAsync("Note.NotFound");

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("2147483648")]
    public Task MalformedNoteId_ReturnsStandardBadRequest(string id)
        => Api.Client.GetAsync($"/api/delivery-notes/{id}")
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);

    [Theory]
    [InlineData("packageDetail.tareWgt", null, HttpStatusCode.BadRequest, "Note.InvalidInput")]
    [InlineData("recipientId", "123456", HttpStatusCode.NotFound, "Recipient.NotFound")]
    [InlineData("contactInfo.id", "99", HttpStatusCode.BadRequest, CommonWebErrorCodes.InvalidHttpRequest)]
    public async Task RejectedCreation_LeavesNoRetrievableNote(
        string path,
        string? json,
        HttpStatusCode expectedStatus,
        string expectedCode
    )
    {
        await this.SetNoteIdentity(0);
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());
        var request = NoteRequests.Valid(recipient.Id);
        NoteRequests.Set(request, path, json is null ? null : JsonNode.Parse(json));

        using var rejected = await Api.Notes().PostRawAsync(request);

        var error = await rejected.AssertErrorAsync(expectedStatus);
        error.ErrorCode.Should().Be(expectedCode);
        await Api.Notes(1).GetRawAsync().AssertNotFoundAsync("Note.NotFound");
    }
}