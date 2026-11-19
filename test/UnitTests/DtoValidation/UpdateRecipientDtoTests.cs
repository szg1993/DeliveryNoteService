using ApiTests.DtoFactories;
using Common.Core.Exceptions;
using Common.Testing.TestSuite;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.UseCases.Recipients.Data;

namespace UnitTests.DtoValidation;

public class UpdateRecipientDtoTests
{
    [Fact]
    public void ValidDetails()
    {
        DtoFactory
            .UpdateRecipient()
            .AssertValid();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NameMissing(string? name)
    {
        (DtoFactory.UpdateRecipient() with { Name = name! })
            .AssertFieldMissing(nameof(UpdateRecipientDto.Name));
    }

    [Fact]
    public void AddressTooLong()
    {
        (DtoFactory.UpdateRecipient() with { Address = new string('A', 201) })
            .AssertFieldTooLong(nameof(UpdateRecipientDto.Address));
    }

    [Fact]
    public void WhitespaceOnlyTextAndUnnormalizedCountryAreAccepted()
    {
        (DtoFactory.UpdateRecipient() with { Name = "   ", Country = " us ", Zip = "   ", City = "   ", Address = "   " })
            .AssertValid();
    }

    [Theory]
    [InlineData("ZZ")]
    [InlineData("HUN")]
    public void CountryUnrecognized(string country)
    {
        (DtoFactory.UpdateRecipient() with { Country = country })
            .AssertDataValidationException(ErrorCodes.Recipient.InvalidCountry);
    }

    [Fact]
    public void MissingProperties()
    {
        new UpdateRecipientDto()
            .AssertDataValidationException(CommonErrorCodes.FieldMissing);
    }
}