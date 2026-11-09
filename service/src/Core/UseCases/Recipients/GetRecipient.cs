using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Application.Errors;
using DeliveryNoteService.Application.UseCases.Recipients;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

[AutoRegister]
public class GetRecipient(IRepository<Recipient> recipients)
{
    public async Task<Envelope<RecipientDetailsDto>> RunAsync(int id, CancellationToken ct)
    {
        if (id <= 0)
            throw new DataValidationException(ErrorCodes.InvalidRecipientId, "Recipient ID must be positive.");

        var recipient = await recipients.GetAsync(id, ct)
            ?? throw new NotFoundException(ErrorCodes.RecipientNotFound, "Recipient not found.");

        return new Envelope<RecipientDetailsDto>(RecipientDetailsDto.FromModel(recipient));
    }
}