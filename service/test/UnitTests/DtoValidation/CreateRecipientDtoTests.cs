using ApiTests.DtoFactories;
using Common.Core.Exceptions;
using Common.Testing.TestSuite;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.UseCases.Recipients;

namespace UnitTests.DtoValidation;

public class CreateRecipientDtoTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CodeMissing(string? code)
    {
        DtoFactory
            .CreateRecipient()
            .WithCode(code!)
            .AssertFieldMissing(nameof(CreateRecipientDto.Code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NameMissing(string? name)
    {
        DtoFactory
            .CreateRecipient()
            .WithName(name!)
            .AssertFieldMissing(nameof(CreateRecipientDto.Name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CountryMissing(string? country)
    {
        DtoFactory
            .CreateRecipient()
            .WithCountry(country!)
            .AssertFieldMissing(nameof(CreateRecipientDto.Country));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ZipMissing(string? zip)
    {
        DtoFactory
            .CreateRecipient()
            .WithZip(zip!)
            .AssertFieldMissing(nameof(CreateRecipientDto.Zip));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CityMissing(string? city)
    {
        DtoFactory
            .CreateRecipient()
            .WithCity(city!)
            .AssertFieldMissing(nameof(CreateRecipientDto.City));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddressMissing(string? address)
    {
        DtoFactory
            .CreateRecipient()
            .WithAddress(address!)
            .AssertFieldMissing(nameof(CreateRecipientDto.Address));
    }

    [Fact]
    public void CodeTooLong()
    {
        DtoFactory
            .CreateRecipient()
            .WithCode(new string('A', 11))
            .AssertFieldTooLong(nameof(CreateRecipientDto.Code));
    }

    [Fact]
    public void NameTooLong()
    {
        DtoFactory
            .CreateRecipient()
            .WithName(new string('A', 201))
            .AssertFieldTooLong(nameof(CreateRecipientDto.Name));
    }

    [Fact]
    public void ZipTooLong()
    {
        DtoFactory
            .CreateRecipient()
            .WithZip(new string('A', 21))
            .AssertFieldTooLong(nameof(CreateRecipientDto.Zip));
    }

    [Fact]
    public void CityTooLong()
    {
        DtoFactory
            .CreateRecipient()
            .WithCity(new string('A', 51))
            .AssertFieldTooLong(nameof(CreateRecipientDto.City));
    }

    [Fact]
    public void AddressTooLong()
    {
        DtoFactory
            .CreateRecipient()
            .WithAddress(new string('A', 201))
            .AssertFieldTooLong(nameof(CreateRecipientDto.Address));
    }

    [Fact]
    public void CodeMaximumLength()
    {
        DtoFactory
            .CreateRecipient()
            .WithCode(new string('A', 10))
            .AssertValid();
    }

    [Fact]
    public void NameMaximumLength()
    {
        DtoFactory
            .CreateRecipient()
            .WithName(new string('A', 200))
            .AssertValid();
    }

    [Fact]
    public void ZipMaximumLength()
    {
        DtoFactory
            .CreateRecipient()
            .WithZip(new string('A', 20))
            .AssertValid();
    }

    [Fact]
    public void CityMaximumLength()
    {
        DtoFactory
            .CreateRecipient()
            .WithCity(new string('A', 50))
            .AssertValid();
    }

    [Fact]
    public void AddressMaximumLength()
    {
        DtoFactory
            .CreateRecipient()
            .WithAddress(new string('A', 200))
            .AssertValid();
    }

    [Fact]
    public void CodeLengthIncludesWhitespace()
    {
        DtoFactory
            .CreateRecipient()
            .WithCode($" {new string('A', 10)} ")
            .AssertFieldTooLong(nameof(CreateRecipientDto.Code));
    }

    [Fact]
    public void NameLengthIncludesWhitespace()
    {
        DtoFactory
            .CreateRecipient()
            .WithName($" {new string('A', 200)} ")
            .AssertFieldTooLong(nameof(CreateRecipientDto.Name));
    }

    [Fact]
    public void ZipLengthIncludesWhitespace()
    {
        DtoFactory
            .CreateRecipient()
            .WithZip($" {new string('A', 20)} ")
            .AssertFieldTooLong(nameof(CreateRecipientDto.Zip));
    }

    [Fact]
    public void CityLengthIncludesWhitespace()
    {
        DtoFactory
            .CreateRecipient()
            .WithCity($" {new string('A', 50)} ")
            .AssertFieldTooLong(nameof(CreateRecipientDto.City));
    }

    [Fact]
    public void AddressLengthIncludesWhitespace()
    {
        DtoFactory
            .CreateRecipient()
            .WithAddress($" {new string('A', 200)} ")
            .AssertFieldTooLong(nameof(CreateRecipientDto.Address));
    }

    [Theory]
    [InlineData("AC ME")]
    [InlineData("ACME!")]
    [InlineData("ÁCME")]
    [InlineData("AC.ME")]
    public void CodeInvalidCharacters(string code)
    {
        DtoFactory
            .CreateRecipient()
            .WithCode(code)
            .AssertDataValidationException(ErrorCodes.Recipient.InvalidCode);
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("HUN")]
    [InlineData("H")]
    [InlineData("12")]
    public void CountryUnrecognized(string country)
    {
        DtoFactory
            .CreateRecipient()
            .WithCountry(country)
            .AssertDataValidationException(ErrorCodes.Recipient.InvalidCountry);
    }

    [Fact]
    public void MissingProperties()
    {
        new CreateRecipientDto()
            .AssertDataValidationException(CommonErrorCodes.FieldMissing);
    }

    [Fact]
    public void PostalFormatAndAddressAreNotVerified()
    {
        DtoFactory
            .CreateRecipient()
            .WithCountry("AQ")
            .WithZip("No postcode")
            .WithCity("Research station")
            .WithAddress("No mapped road")
            .AssertValid();
    }
}