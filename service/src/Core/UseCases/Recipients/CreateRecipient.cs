using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

[AutoRegister]
public class CreateRecipient(
    IRepository<Recipient> repository,
    IUnitOfWork unitOfWork,
    ITimeService timeService,
    TechnicalUseCaseLogger<CreateRecipient> logger
)
{
    public Task<Envelope<RecipientDetailsDto>> RunAsync(
        CreateRecipientDto dto, CancellationToken ct
    )
    {
        return logger.RunAsync(async () =>
        {
            dto.Validate();

            var code = dto.Code.Trim().ToUpperInvariant();

            if (await repository.AnyAsync(recipient => recipient.Code == code, ct))
                throw new DataValidationException(ErrorCodes.Recipient.InvalidCode);

            var recipient = dto.ToModel(timeService);

            repository.Add(recipient);

            await unitOfWork.SaveChangesAsync(ct);

            return RecipientDetailsDto.FromModel(recipient).Envelop();
        });
    }
}