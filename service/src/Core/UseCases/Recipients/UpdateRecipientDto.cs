using Common.Core.UseCases;
using DeliveryNoteService.Core.Models;
using System.ComponentModel;

namespace DeliveryNoteService.Core.UseCases.Recipients;

public record UpdateRecipientDto : IValidatable
{
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
        Guard.ThrowIfMissing(Name);
        Guard.ThrowIfTooLong(Name, RecipientLimits.Name);
        Guard.ThrowIfMissing(Country);
        Guard.ThrowIfMissing(Zip);
        Guard.ThrowIfTooLong(Zip, RecipientLimits.Zip);
        Guard.ThrowIfMissing(City);
        Guard.ThrowIfTooLong(City, RecipientLimits.City);
        Guard.ThrowIfMissing(Address);
        Guard.ThrowIfTooLong(Address, RecipientLimits.Address);

        RecipientCountries.ThrowIfUnrecognized(Country);
    }

    public Recipient MapTo(Recipient model)
    {
        model.Name = Name;
        model.Country = RecipientCountries.Normalize(Country);
        model.Zip = Zip;
        model.City = City;
        model.Address = Address;

        return model;
    }
}