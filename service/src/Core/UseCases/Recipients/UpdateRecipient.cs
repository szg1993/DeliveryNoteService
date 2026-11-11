using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

[AutoRegister]
public class UpdateRecipient(
    IRepository<Recipient> repository,
    IUnitOfWork unitOfWork,
    TechnicalUseCaseLogger<UpdateRecipient> logger
)
{
    public Task<Envelope<RecipientDetailsDto>> RunAsync(
        int id,
        UpdateRecipientDto dto,
        CancellationToken ct
    )
    {
        return logger.RunAsync(async () =>
        {
            dto.Validate();

            var recipient = await repository.GetAsync(id, ct)
                ?? throw new NotFoundException(ErrorCodes.Recipient.NotFound, "Recipient not found.");

            dto.MapTo(recipient);

            await unitOfWork.SaveChangesAsync(ct);

            return RecipientDetailsDto.FromModel(recipient).Envelop();
        });
    }
}