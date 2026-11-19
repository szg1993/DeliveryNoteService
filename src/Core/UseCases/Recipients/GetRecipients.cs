using Common.Core.DependencyInjection;
using Common.Core.Logging;
using Common.Core.Messaging;
using Common.Core.Persistence;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Models;
using DeliveryNoteService.Core.UseCases.Recipients.Data;

namespace DeliveryNoteService.Core.UseCases.Recipients;

public enum RecipientsSortBy
{
    Id,
    Name
}

[AutoRegister]
public class GetRecipients(
    IRepository<Recipient> recipients,
    TechnicalUseCaseLogger<GetRecipients> logger
)
{
    private static SortMap<Recipient> SortMap { get; } = new SortMap<Recipient>()
        .Add(RecipientsSortBy.Id, recipient => recipient.Id)
        .Add(nameof(RecipientsSortBy.Name), SortByName);

    public Task<PagedEnvelope<RecipientDetailsDto>> RunAsync(
        PageRequest pageRequest,
        Sorting<RecipientsSortBy> sorting,
        CancellationToken ct
    )
    {
        return logger.RunAsync(async () =>
        {
            var effectiveSorting = sorting with { SortBy = sorting.SortBy ?? RecipientsSortBy.Id };

            return await recipients
                .Paged(pageRequest, SortMap.Get(effectiveSorting), ct)
                .As(RecipientDetailsDto.FromModel);
        });
    }

    private static IOrderedQueryable<Recipient> SortByName(IQueryable<Recipient> query, SortDirection direction)
        => direction == SortDirection.Desc
            ? query.OrderByDescending(recipient => recipient.Name).ThenBy(recipient => recipient.Id)
            : query.OrderBy(recipient => recipient.Name).ThenBy(recipient => recipient.Id);
}