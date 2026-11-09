using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Application.Errors;
using DeliveryNoteService.Application.UseCases.Recipients;
using DeliveryNoteService.Core.Models;
using System.ComponentModel;

namespace DeliveryNoteService.Core.UseCases.Recipients;

public record CreateRecipientDto : IValidatable
{
    [Description("Manually entered unique code. Trimmed and uppercased; 1-10 ASCII letters, digits, hyphens, or underscores after normalization.")]
    public string Code { get; init; } = null!;

    [Description("Recipient name, up to 200 characters after trimming. Duplicate names are allowed.")]
    public string Name { get; init; } = null!;

    [Description("Recognized two-letter country code, trimmed and uppercased. HU is domestic; other countries are foreign.")]
    public string Country { get; init; } = null!;

    [Description("ZIP or postal code as text, up to 20 characters after trimming. Leading zeros are preserved; no postal-format verification is performed.")]
    public string Zip { get; init; } = null!;

    [Description("Destination city, up to 50 characters after trimming.")]
    public string City { get; init; } = null!;

    [Description("Destination address, up to 200 characters after trimming. Address existence is not verified.")]
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

        if (!code.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_'))
            throw new DataValidationException(ErrorCodes.InvalidRecipientCode, "Recipient code contains unsupported characters.");

        var country = Country.Trim().ToUpperInvariant();

        if (!RecipientCountries.Contains(country))
            throw new DataValidationException(ErrorCodes.InvalidRecipientCountry, "Country must be a recognized two-letter code.");
    }

    //public Recipient ToModel()
    //    => MapTo(new Recipient());

    //public Recipient MapTo(Recipient model)
    //{
    //    model.Code = Code.Trim().ToUpperInvariant();
    //    model.Name = Name.Trim();
    //    model.Country = Country.Trim().ToUpperInvariant();
    //    model.Zip = .Zip.Trim();
    //    model.City = dto.City.Trim();
    //    model.Address = dto.Address.Trim();
    //    model.CreatedAt = time.UtcNow;
    //}
}