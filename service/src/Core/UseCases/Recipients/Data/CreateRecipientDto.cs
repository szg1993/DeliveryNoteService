using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;
using System.ComponentModel;

namespace DeliveryNoteService.Core.UseCases.Recipients.Data;

public record CreateRecipientDto : UpdateRecipientDto
{
    [Description("Manually entered unique code, up to 10 characters as supplied. Trimmed and uppercased; must be non-empty and contain only ASCII letters, digits, hyphens, or underscores after normalization.")]
    public string Code { get; init; } = null!;

    public override void Validate()
    {
        Guard.ThrowIfMissing(Code);
        Guard.ThrowIfTooLong(Code, RecipientLimits.Code);

        var code = Code.Trim().ToUpperInvariant();

        if (code.Length == 0 || !code.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_'))
            throw new DataValidationException(ErrorCodes.Recipient.InvalidCode, "Recipient code must be non-empty and contain only ASCII letters, digits, hyphens, or underscores.");

        base.Validate();
    }

    public Recipient ToModel(ITimeService timeService)
        => MapTo(new Recipient(), timeService);

    public Recipient MapTo(Recipient model, ITimeService timeService)
    {
        model.Code = Code.Trim().ToUpperInvariant();
        model.Name = Name;
        model.Country = RecipientCountries.Normalize(Country);
        model.Zip = Zip;
        model.City = City;
        model.Address = Address;
        model.CreatedAt = timeService.UtcNow;

        return model;
    }
}