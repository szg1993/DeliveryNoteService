using Medusa.Enap.Common.AspNetCore;
using Medusa.Enap.Common.Core.Exceptions;
using System.Net;
using System.Text;

namespace ApiTests;

public class RecipientCreationTests : DemoServiceApiTestBase
{
    [Fact]
    public async Task CreateRecipient_ReturnsCreatedWithLocation()
    {
        var request = RecipientRequests.Valid();

        var response = await Api.Recipients().PostRawAsync(request);

        await response.AssertStatusCode(HttpStatusCode.Created);
        Assert.NotNull(response.Headers.Location);
    }

    [Theory]
    [InlineData(" hu ", "HU", false)]
    [InlineData(" us ", "US", true)]
    public async Task CreateRecipient_NormalizesDetailsAndDerivesForeignStatus(string country, string expectedCountry, bool isForeign)
    {
        var request = RecipientRequests.Valid() with
        {
            Code = " acme-01 ",
            Name = " Acme  Ltd ",
            Country = country,
            Zip = " 001AB ",
            City = " New  York ",
            Address = " Main  street 1 "
        };

        var created = await Api.Recipients().PostAsync(request);

        created.Code.Should().Be("ACME-01");
        created.Name.Should().Be("Acme  Ltd");
        created.Country.Should().Be(expectedCountry);
        created.Zip.Should().Be("001AB");
        created.City.Should().Be("New  York");
        created.Address.Should().Be("Main  street 1");
        created.IsForeign.Should().Be(isForeign);
        (await Api.Recipients(created.Id).GetAsync()).Should().BeEquivalentTo(created);
    }

    [Theory]
    [InlineData("Code")]
    [InlineData("Name")]
    [InlineData("Country")]
    [InlineData("Zip")]
    [InlineData("City")]
    [InlineData("Address")]
    public async Task CreateRecipient_RejectsBlankRequiredField(string field)
    {
        var request = RecipientRequests.WithField(field, " \t ");

        var error = await Api.Recipients().PostRawAsync(request)
            .AssertBadRequestAsync(CommonErrorCodes.FieldMissing);

        error.Data.Should().ContainKey("Field");
        error.Data["Field"].ToString().Should().Be(field);
        (await Api.Recipients().PostAsync(RecipientRequests.Valid())).Code.Should().Be("ACME");
    }

    [Theory]
    [InlineData("Code", 11)]
    [InlineData("Name", 201)]
    [InlineData("Zip", 21)]
    [InlineData("City", 51)]
    [InlineData("Address", 201)]
    public async Task CreateRecipient_RejectsOverlengthField(string field, int length)
    {
        var request = RecipientRequests.WithField(field, new string('A', length));

        await Api.Recipients().PostRawAsync(request)
            .AssertBadRequestAsync(CommonErrorCodes.FieldTooLong);

        (await Api.Recipients().PostAsync(RecipientRequests.Valid())).Code.Should().Be("ACME");
    }

    [Theory]
    [InlineData("AC ME")]
    [InlineData("ACME!")]
    [InlineData("ÁCME")]
    [InlineData("AC.ME")]
    public async Task CreateRecipient_RejectsUnsupportedCodeCharacters(string code)
    {
        var request = RecipientRequests.Valid(code: code);

        await Api.Recipients().PostRawAsync(request)
            .AssertBadRequestAsync("InvalidRecipientCode");
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("HUN")]
    [InlineData("H")]
    [InlineData("12")]
    public async Task CreateRecipient_RejectsUnrecognizedCountry(string country)
    {
        var request = RecipientRequests.Valid() with { Country = country };

        await Api.Recipients().PostRawAsync(request)
            .AssertBadRequestAsync("InvalidRecipientCountry");

        (await Api.Recipients().PostAsync(RecipientRequests.Valid())).Code.Should().Be("ACME");
    }

    [Theory]
    [InlineData("ACME")]
    [InlineData("acme")]
    [InlineData(" ACME ")]
    public async Task CreateRecipient_RejectsExistingNormalizedCode(string code)
    {
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        var error = await Api.Recipients().PostRawAsync(RecipientRequests.Valid(code: code))
            .AssertErrorAsync(HttpStatusCode.Conflict);

        error.ErrorCode.Should().Be(CommonErrorCodes.UniqueKeyConflict);
        (await Api.Recipients(original.Id).GetAsync()).Should().BeEquivalentTo(original);
    }

    [Theory]
    [InlineData("Code")]
    [InlineData("Name")]
    [InlineData("Country")]
    [InlineData("Zip")]
    [InlineData("City")]
    [InlineData("Address")]
    public Task CreateRecipient_RejectsNullRequiredField(string field)
        => Api.Recipients().PostRawAsync(RecipientRequests.WithField(field, null))
            .AssertBadRequestAsync(CommonErrorCodes.FieldMissing);

    [Fact]
    public Task CreateRecipient_RejectsMissingFields()
        => Api.Recipients().PostRawAsync(new { })
            .AssertBadRequestAsync(CommonErrorCodes.FieldMissing);

    [Theory]
    [InlineData("Code", 10)]
    [InlineData("Name", 200)]
    [InlineData("Zip", 20)]
    [InlineData("City", 50)]
    [InlineData("Address", 200)]
    public async Task CreateRecipient_AcceptsMaximumLengthAfterTrimming(string field, int length)
    {
        var value = new string('A', length);
        var request = RecipientRequests.WithField(field, $" {value} ");

        var created = await Api.Recipients().PostAsync(request);

        created.Should().BeEquivalentTo(RecipientRequests.WithField(field, value));
        (await Api.Recipients(created.Id).GetAsync()).Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task CreateRecipient_AllowsDuplicateNamesWithDifferentCodes()
    {
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid(code: "ACME-01"));

        var other = await Api.Recipients().PostAsync(RecipientRequests.Valid(code: "ACME_02"));

        other.Name.Should().Be("Acme Ltd");
        other.Id.Should().NotBe(original.Id);
        (await Api.Recipients(other.Id).GetAsync()).Code.Should().Be("ACME_02");
    }

    [Fact]
    public async Task CreateRecipient_IgnoresClientAssignedServerFields()
    {
        var createdAt = new DateTimeOffset(2026, 11, 8, 13, 0, 0, TimeSpan.Zero);
        this.Time().SetDateTime(createdAt);
        var request = new
        {
            Code = "ACME",
            Name = "Acme Ltd",
            Country = "HU",
            Zip = "0101",
            City = "Budapest",
            Address = "Main street 1",
            Id = -99,
            CreatedAt = "2000-01-01T00:00:00Z",
            IsForeign = true
        };

        var created = await Api.Recipients().PostAsync(request);

        created.Id.Should().BePositive();
        created.CreatedAt.Should().Be(createdAt);
        created.IsForeign.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRecipient_DoesNotVerifyPostalFormatOrAddressExistence()
    {
        var request = RecipientRequests.Valid() with
        {
            Country = "AQ",
            Zip = "No postcode",
            City = "Research station",
            Address = "No mapped road"
        };

        var created = await Api.Recipients().PostAsync(request);

        created.Should().BeEquivalentTo(request);
        created.IsForeign.Should().BeTrue();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    public Task CreateRecipient_RejectsMissingOrMalformedJsonBody(string json)
        => Api.Client.PostAsync("/api/recipients", new StringContent(json, Encoding.UTF8, "application/json"))
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
}