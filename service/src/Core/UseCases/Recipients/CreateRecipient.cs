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
                throw RecipientAlreadyExists();

            var recipient = dto.ToModel(timeService);

            repository.Add(recipient);

            try
            {
                await unitOfWork.SaveChangesAsync(ct);
            }
            catch (UniqueKeyConflictException ex)
            {
                throw RecipientAlreadyExists(ex);
            }

            return RecipientDetailsDto.FromModel(recipient).Envelop();
        });
    }

    private static DataValidationException RecipientAlreadyExists(Exception? inner = null)
        => new(ErrorCodes.Recipient.AlreadyExists, "A recipient with this code already exists.", inner);
}