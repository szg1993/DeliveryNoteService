using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Application.UseCases.Recipients;

public record RecipientDetailsDto(
    int Id,
    string Code,
    string Name,
    string Country,
    string Zip,
    string City,
    string Address,
    DateTimeOffset CreatedAt,
    bool IsForeign
)
{
    public static RecipientDetailsDto FromModel(Recipient recipient)
        => new(
            recipient.Id,
            recipient.Code!,
            recipient.Name!,
            recipient.Country!,
            recipient.Zip!,
            recipient.City!,
            recipient.Address!,
            recipient.CreatedAt,
            recipient.Country != "HU"
        );
}