using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Infrastructure;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Application.Errors;
using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application.Recipients;

public class CreateRecipient(
    IRepository<Recipient> recipients,
    IUnitOfWork unitOfWork,
    ITimeService time
) : IUseCase
{
    public async Task<Envelope<RecipientDetails>> RunAsync(CreateRecipientRequest request, CancellationToken ct)
    {
        var recipient = new Recipient
        {
            Code = Required(request.Code, nameof(request.Code), RecipientLimits.Code).ToUpperInvariant(),
            Name = Required(request.Name, nameof(request.Name), RecipientLimits.Name),
            Country = Required(request.Country, nameof(request.Country)).ToUpperInvariant(),
            Zip = Required(request.Zip, nameof(request.Zip), RecipientLimits.Zip),
            City = Required(request.City, nameof(request.City), RecipientLimits.City),
            Address = Required(request.Address, nameof(request.Address), RecipientLimits.Address),
            CreatedAt = time.UtcNow
        };
        if (!recipient.Code.All(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_'))
            throw new DataValidationException(ErrorCodes.InvalidRecipientCode, "Recipient code contains unsupported characters.");
        if (!RecipientCountries.Contains(recipient.Country))
            throw new DataValidationException(ErrorCodes.InvalidRecipientCountry, "Country must be a recognized two-letter code.");
        recipients.Add(recipient);
        await unitOfWork.SaveChangesAsync(ct);
        return new Envelope<RecipientDetails>(RecipientDetails.From(recipient));
    }

    private static string Required(string? value, string field, int maxLength = int.MaxValue)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
            throw new FieldMissingException(field);
        if (normalized.Length > maxLength)
            throw new FieldTooLongException(field, maxLength);
        return normalized;
    }
}