using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

[AutoRegister]
public class GetDeliveryNote(
    IRepository<Note> repository,
    TechnicalUseCaseLogger<GetDeliveryNote> logger
)
{
    public Task<Envelope<NoteDetailsDto>> RunAsync(int id, CancellationToken ct)
    {
        return logger.RunAsync(async () =>
        {
            var note = await repository.NoTracking()
                .Include(value => value.Recipient)
                .Include(value => value.ContactInfo)
                .Include(value => value.PackageDetail)
                .Include(value => value.Deadline)
                .Include(value => value.NoteLines)
                .GetAsync(id, ct)
                ?? throw new NotFoundException(ErrorCodes.Note.NotFound);

            return NoteDetailsDto.FromModel(note).Envelop();
        });
    }
}