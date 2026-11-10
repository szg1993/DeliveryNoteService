using Common.AspNetCore;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using System.Net;
using System.Text.Json.Nodes;

namespace ApiTests;

public class NoteDecimalTests : DeliveryNoteServiceApiTestBase
{
    [Theory]
    [MemberData(nameof(UnrepresentableNumbers))]
    public async Task UnrepresentableNumber_ReturnsBadRequestWithoutCreatingNote(string path, string number)
    {
        await this.SetNoteIdentity(0);
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        using var response = await Api.PostRawNumberAsync(recipient.Id, path, number);

        await response.AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
        await Api.Notes(1).GetRawAsync().AssertNotFoundAsync("Note.NotFound");
    }

    [Theory]
    [MemberData(nameof(ExactNumbers))]
    public async Task ExactlyRepresentableNumber_IsSavedWithoutChangingItsValue(
        string path,
        string number,
        decimal expected
    )
    {
        var recipient = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        using var response = await Api.PostRawNumberAsync(recipient.Id, path, number);

        await response.AssertStatusCode(HttpStatusCode.Created);
        var created = (await response.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        using var savedResponse = await Api.Notes(created.Id).GetRawAsync();
        await savedResponse.AssertStatusCode(HttpStatusCode.OK);
        var saved = (await savedResponse.ReadAsync<JsonObject>())["content"]!;
        NoteRequests.Get(saved, path)!.GetValue<decimal>().Should().Be(expected);
    }

    public static IEnumerable<object[]> UnrepresentableNumbers()
    {
        var numbers = new[]
        {
            "0.12345678901234567890123456789",
            "0.00000000000000000000000000001",
            "-0.00000000000000000000000000001",
            "79228162514264337593543950336",
            "1e999999",
            "1e-999999"
        };
        foreach (var path in NumericFields())
        {
            foreach (var number in numbers)
                yield return new object[] { path, number };
        }
    }

    public static IEnumerable<object[]> ExactNumbers()
    {
        var positiveNumbers = new (string Json, decimal Expected)[]
        {
            ("0.1234567890123456789012345678", 0.1234567890123456789012345678m),
            ("1e-28", 0.0000000000000000000000000001m),
            ("10e-29", 0.0000000000000000000000000001m),
            ("1.2300e2", 123m),
            ("1e+2", 100m),
            ("79228162514264337593543950335", decimal.MaxValue),
            ("792281625142643375935439503350e-1", decimal.MaxValue),
            ("1.00000000000000000000000000000", 1m)
        };
        foreach (var path in NumericFields())
        {
            foreach (var (number, expected) in positiveNumbers)
                yield return new object[] { path, number, expected };
        }
        foreach (var path in NumericFields().Where(path => !path.EndsWith("Qty")))
        {
            foreach (var number in new[] { "0e9999999999999999999999999999", "-0e-9999", "0.00000000000000000000000000000" })
                yield return new object[] { path, number, 0m };
        }
    }

    private static IEnumerable<string> NumericFields()
    {
        return new[]
        {
            "packageDetail.tareWgt",
            "packageDetail.pkgQty",
            "packageDetail.pkgSizeX",
            "packageDetail.pkgSizeY",
            "packageDetail.pkgSizeZ",
            "noteLines.0.partQty",
            "noteLines.0.partWgt"
        };
    }
}