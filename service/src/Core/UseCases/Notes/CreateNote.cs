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
public class CreateNote(
    IRepository<Note> notes,
    IRepository<Recipient> recipients,
    IUnitOfWork unitOfWork,
    ITimeService timeService,
    TechnicalUseCaseLogger<CreateNote> logger
)
{
    public Task<Envelope<NoteDetailsDto>> RunAsync(CreateNoteDto dto, CancellationToken ct)
    {
        return logger.RunAsync(async () =>
        {
            try
            {
                dto.Validate();
            }
            catch (DataValidationException exception) when (exception is FieldMissingException or FieldTooLongException)
            {
                throw new DataValidationException(ErrorCodes.Note.InvalidInput, exception.Message, exception);
            }
            var recipient = await recipients.GetAsync(dto.RecipientId, ct)
                ?? throw new NotFoundException(ErrorCodes.Recipient.NotFound, "Recipient not found.");
            var note = dto.ToModel(timeService);
            note.Recipient = recipient;
            notes.Add(note);
            await unitOfWork.SaveChangesAsync(ct);
            return NoteDetailsDto.FromModel(note).Envelop();
        });
    }
}