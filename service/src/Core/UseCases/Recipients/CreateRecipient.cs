using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Application.UseCases.Recipients;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

[AutoRegister]
public class CreateRecipient(
    IRepository<Recipient> recipientRepository,
    IUnitOfWork unitOfWork,
    ITimeService time,
    TechnicalUseCaseLogger<CreateRecipient> logger
)
{
    public Task<Envelope<RecipientDetailsDto>> RunAsync(CreateRecipientDto dto, CancellationToken ct)
    {
        return logger.RunAsync(async () =>
        {
            dto.Validate();

            var code = dto.Code.Trim().ToUpperInvariant();

            if (await recipientRepository.AnyAsync(recipient => recipient.Code == code, ct))
                throw new UniqueKeyConflictException(null);

            var recipient = new Recipient
            {
                Code = code,
                Name = dto.Name.Trim(),
                Country = dto.Country.Trim().ToUpperInvariant(),
                Zip = dto.Zip.Trim(),
                City = dto.City.Trim(),
                Address = dto.Address.Trim(),
                CreatedAt = time.UtcNow
            };

            recipientRepository.Add(recipient);
            await unitOfWork.SaveChangesAsync(ct);

            return RecipientDetailsDto.FromModel(recipient).Envelop();
        });
    }
}