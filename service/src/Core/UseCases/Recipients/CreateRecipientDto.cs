using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;
using System.ComponentModel;

namespace DeliveryNoteService.Core.UseCases.Recipients;

public record CreateRecipientDto : IValidatable
{
    [Description("Manually entered unique code, up to 10 characters as supplied. Trimmed and uppercased; must be non-empty and contain only ASCII letters, digits, hyphens, or underscores after normalization.")]
    public string Code { get; init; } = null!;

    [Description("Recipient name, up to 200 characters, stored exactly as supplied. Duplicate names are allowed.")]
    public string Name { get; init; } = null!;

    [Description("Recognized two-letter country code, trimmed and uppercased. HU is domestic; other countries are foreign.")]
    public string Country { get; init; } = null!;

    [Description("ZIP or postal code as text, up to 20 characters, stored exactly as supplied. Leading zeros are preserved; no postal-format verification is performed.")]
    public string Zip { get; init; } = null!;

    [Description("Destination city, up to 50 characters, stored exactly as supplied.")]
    public string City { get; init; } = null!;

    [Description("Destination address, up to 200 characters, stored exactly as supplied. Address existence is not verified.")]
    public string Address { get; init; } = null!;

    public void Validate()
    {
        Guard.ThrowIfMissing(Code);
        Guard.ThrowIfTooLong(Code, RecipientLimits.Code);
        Guard.ThrowIfMissing(Name);
        Guard.ThrowIfTooLong(Name, RecipientLimits.Name);
        Guard.ThrowIfMissing(Country);
        Guard.ThrowIfMissing(Zip);
        Guard.ThrowIfTooLong(Zip, RecipientLimits.Zip);
        Guard.ThrowIfMissing(City);
        Guard.ThrowIfTooLong(City, RecipientLimits.City);
        Guard.ThrowIfMissing(Address);
        Guard.ThrowIfTooLong(Address, RecipientLimits.Address);

        var code = Code.Trim().ToUpperInvariant();

        if (code.Length == 0 || !code.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_'))
            throw new DataValidationException(ErrorCodes.Recipient.InvalidCode, "Recipient code must be non-empty and contain only ASCII letters, digits, hyphens, or underscores.");

        var country = Country.Trim().ToUpperInvariant();

        if (!RecipientCountries.Contains(country))
            throw new DataValidationException(ErrorCodes.Recipient.InvalidCountry, "Country must be a recognized two-letter code.");
    }

    public Recipient ToModel(ITimeService timeService)
        => MapTo(new Recipient(), timeService);

    public Recipient MapTo(Recipient model, ITimeService timeService)
    {
        model.Code = Code.Trim().ToUpperInvariant();
        model.Name = Name;
        model.Country = Country.Trim().ToUpperInvariant();
        model.Zip = Zip;
        model.City = City;
        model.Address = Address;
        model.CreatedAt = timeService.UtcNow;

        return model;
    }
}