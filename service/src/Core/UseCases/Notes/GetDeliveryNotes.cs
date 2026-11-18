using Common.Core.DependencyInjection;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public enum DeliveryNotesSortBy
{
    Id
}

[AutoRegister]
public class GetDeliveryNotes(
    IRepository<Note> repository,
    TechnicalUseCaseLogger<GetDeliveryNotes> logger
)
{
    private static SortMap<Note> SortMap { get; } = new SortMap<Note>()
        .Add(DeliveryNotesSortBy.Id, note => note.Id);

    public Task<PagedEnvelope<NoteDetailsDto>> RunAsync(
        PageRequest pageRequest,
        Sorting<DeliveryNotesSortBy> sorting,
        CancellationToken ct
    )
    {
        return logger.RunAsync(async () =>
        {
            var effectiveSorting = sorting with { SortBy = sorting.SortBy ?? DeliveryNotesSortBy.Id };

            return await repository
                .NoTracking()
                .IncludeDetails()
                .Paged(pageRequest, SortMap.Get(effectiveSorting), ct)
                .As(NoteDetailsDto.FromModel);
        });
    }
}