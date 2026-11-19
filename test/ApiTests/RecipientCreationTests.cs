using Common.AspNetCore;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
using System.Net;
using System.Text;

namespace ApiTests;

public class RecipientCreationTests : DeliveryNoteServiceApiTestBase
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
    public async Task CreateRecipient_NormalizesIdentifiersPreservesTextAndDerivesForeignStatus(string country, string expectedCountry, bool isForeign)
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
        created.Name.Should().Be(" Acme  Ltd ");
        created.Country.Should().Be(expectedCountry);
        created.Zip.Should().Be(" 001AB ");
        created.City.Should().Be(" New  York ");
        created.Address.Should().Be(" Main  street 1 ");
        created.IsForeign.Should().Be(isForeign);
        (await Api.Recipients(created.Id).GetAsync()).Should().BeEquivalentTo(created);
    }

    [Theory]
    [InlineData("ACME")]
    [InlineData("acme")]
    [InlineData(" ACME ")]
    public async Task CreateRecipient_RejectsExistingNormalizedCode(string code)
    {
        var original = await Api.Recipients().PostAsync(RecipientRequests.Valid());

        var error = await Api.Recipients().PostRawAsync(RecipientRequests.Valid(code: code))
            .AssertErrorAsync(HttpStatusCode.BadRequest);

        error.ErrorCode.Should().Be("Recipient.AlreadyExists");
        (await Api.Recipients(original.Id).GetAsync()).Should().BeEquivalentTo(original);
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
        var request = new RecipientWithServerFieldsDto
        {
            Code = "ACME",
            Name = "Acme Ltd",
            Country = "HU",
            Zip = "0101",
            City = "Budapest",
            Address = "Main street 1",
            Id = -99,
            CreatedAt = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            IsForeign = true
        };

        var created = await Api.Recipients().PostAsync(request);

        created.Id.Should().BePositive();
        created.CreatedAt.Should().Be(createdAt);
        created.IsForeign.Should().BeFalse();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{")]
    public Task CreateRecipient_RejectsMissingOrMalformedJsonBody(string json)
        => Api.Client.PostAsync("/api/recipients", new StringContent(json, Encoding.UTF8, "application/json"))
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);

    private sealed record RecipientWithServerFieldsDto : CreateRecipientDto
    {
        public int Id { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public bool IsForeign { get; init; }
    }
}