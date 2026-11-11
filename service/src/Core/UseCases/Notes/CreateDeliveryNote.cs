using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

[AutoRegister]
public class CreateDeliveryNote(
    IRepository<Note> noteRepository,
    IRepository<Recipient> recipientRepository,
    IUnitOfWork unitOfWork,
    ITimeService timeService,
    TechnicalUseCaseLogger<CreateDeliveryNote> logger
)
{
    public Task<Envelope<NoteDetailsDto>> RunAsync(CreateDeliveryNoteDto dto, CancellationToken ct)
    {
        return logger.RunAsync(async () =>
        {
            dto.Validate();

            var recipient = await recipientRepository.GetAsync(dto.RecipientId, ct)
                ?? throw new NotFoundException(ErrorCodes.Recipient.NotFound);

            var note = dto.ToModel(timeService);

            note.Recipient = recipient;

            noteRepository.Add(note);

            await unitOfWork.SaveChangesAsync(ct);

            return NoteDetailsDto.FromModel(note).Envelop();
        });
    }
}