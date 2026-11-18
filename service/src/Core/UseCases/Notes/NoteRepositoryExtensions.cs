using Common.Core.Persistence;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

internal static class NoteRepositoryExtensions
{
    public static IReadOnlyRepository<Note> IncludeDetails(this IReadOnlyRepository<Note> repository)
        => repository
            .Include(note => note.Recipient)
            .Include(note => note.ContactInfo)
            .Include(note => note.PackageDetail)
            .Include(note => note.Deadline)
            .Include(note => note.NoteLines);
}