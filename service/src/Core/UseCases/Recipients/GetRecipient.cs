using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

[AutoRegister]
public class GetRecipient(
    IRepository<Recipient> repository,
    TechnicalUseCaseLogger<GetRecipient> logger
)
{
    public Task<Envelope<RecipientDetailsDto>> RunAsync(int id, CancellationToken ct)
    {
        return logger.RunAsync(async () =>
        {
            var recipient = await repository.GetAsync(id, ct)
                ?? throw new NotFoundException(ErrorCodes.Recipient.NotFound, "Recipient not found.");

            return RecipientDetailsDto.FromModel(recipient).Envelop();
        });
    }
}