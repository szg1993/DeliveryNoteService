using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application.Recipients;

public record RecipientDetails(
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
    public static RecipientDetails From(Recipient recipient)
        => new(
            recipient.Id,
            recipient.Code,
            recipient.Name,
            recipient.Country,
            recipient.Zip,
            recipient.City,
            recipient.Address,
            recipient.CreatedAt,
            recipient.Country != "HU"
        );
}