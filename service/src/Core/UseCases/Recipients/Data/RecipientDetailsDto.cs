using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients.Data;

public record RecipientDetailsDto
{
    public int Id { get; init; }

    public string Code { get; init; } = null!;

    public string Name { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string Zip { get; init; } = null!;

    public string City { get; init; } = null!;

    public string Address { get; init; } = null!;

    public DateTimeOffset CreatedAt { get; init; }

    public bool IsForeign { get; init; }

    public static RecipientDetailsDto FromModel(Recipient recipient)
    {
        return new RecipientDetailsDto
        {
            Id = recipient.Id,
            Code = recipient.Code!,
            Name = recipient.Name!,
            Country = recipient.Country!,
            Zip = recipient.Zip!,
            City = recipient.City!,
            Address = recipient.Address!,
            CreatedAt = recipient.CreatedAt,
            IsForeign = recipient.Country != "HU"
        };
    }
}