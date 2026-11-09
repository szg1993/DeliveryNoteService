using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Messaging;
using Common.Core.Persistence;
using Common.Core.UseCases;
using DeliveryNoteService.Application.UseCases.Recipients;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Recipients;

public enum RecipientsSortBy
{
    Id,
    Name
}

[AutoRegister]
public class GetRecipients(IRepository<Recipient> recipients)
{
    private static SortMap<Recipient> SortMap { get; } = new SortMap<Recipient>()
        .Add(RecipientsSortBy.Id, recipient => recipient.Id)
        .Add(nameof(RecipientsSortBy.Name), SortByName);

    public async Task<PagedEnvelope<RecipientDetailsDto>> RunAsync(
        PageRequest pageRequest,
        Sorting<RecipientsSortBy> sorting,
        CancellationToken cancellationToken
    )
    {
        Validate(pageRequest, sorting);

        if (((long)pageRequest.Page - 1) * pageRequest.Limit > int.MaxValue)
        {
            var totalCount = await recipients.CountAsync(query => query, cancellationToken);
            return new PagedEnvelope<RecipientDetailsDto>([], pageRequest, totalCount);
        }

        var effectiveSorting = sorting with { SortBy = sorting.SortBy ?? RecipientsSortBy.Id };
        return await recipients.Paged(pageRequest, SortMap.Get(effectiveSorting), cancellationToken)
            .As(RecipientDetailsDto.FromModel);
    }

    private static void Validate(PageRequest pageRequest, Sorting<RecipientsSortBy> sorting)
    {
        if (pageRequest.Page < 1 || pageRequest.Limit < 0)
        {
            throw new DataValidationException(CommonErrorCodes.InvalidPageRequest);
        }

        if (sorting.SortBy is { } sortBy && !Enum.IsDefined(sortBy))
        {
            throw new DataValidationException(CommonErrorCodes.InvalidSortField);
        }

        if (sorting.SortDir is { } sortDirection && !Enum.IsDefined(sortDirection))
        {
            throw new DataValidationException(CommonErrorCodes.InvalidSortField);
        }
    }

    private static IOrderedQueryable<Recipient> SortByName(IQueryable<Recipient> query, SortDirection direction)
        => direction == SortDirection.Desc
            ? query.OrderByDescending(recipient => recipient.Name).ThenBy(recipient => recipient.Id)
            : query.OrderBy(recipient => recipient.Name).ThenBy(recipient => recipient.Id);
}