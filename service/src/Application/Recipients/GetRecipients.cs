using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application.Recipients;

public enum RecipientsSortBy
{
    Id,
    Name
}

public class GetRecipients(IRepository<Recipient> recipients) : IUseCase
{
    private static SortMap<Recipient> SortMap { get; } = new SortMap<Recipient>()
        .Add(RecipientsSortBy.Id, recipient => recipient.Id)
        .Add(nameof(RecipientsSortBy.Name), SortByName);

    public async Task<PagedEnvelope<RecipientDetails>> RunAsync(
        PageRequest pageRequest,
        Sorting<RecipientsSortBy> sorting,
        CancellationToken ct
    )
    {
        Validate(pageRequest, sorting);
        if (((long)pageRequest.Page - 1) * pageRequest.Limit > int.MaxValue)
        {
            var totalCount = await recipients.CountAsync(query => query, ct);
            return new PagedEnvelope<RecipientDetails>([], pageRequest, totalCount);
        }
        var effectiveSorting = sorting with { SortBy = sorting.SortBy ?? RecipientsSortBy.Id };
        return await recipients.Paged(pageRequest, SortMap.Get(effectiveSorting), ct)
            .As(RecipientDetails.From);
    }

    private static void Validate(PageRequest pageRequest, Sorting<RecipientsSortBy> sorting)
    {
        if (pageRequest.Page < 1 || pageRequest.Limit < 0)
            throw new DataValidationException(CommonErrorCodes.InvalidPageRequest);
        if (sorting.SortBy is { } sortBy && !Enum.IsDefined(sortBy))
            throw new DataValidationException(CommonErrorCodes.InvalidSortField);
        if (sorting.SortDir is { } sortDirection && !Enum.IsDefined(sortDirection))
            throw new DataValidationException(CommonErrorCodes.InvalidSortField);
    }

    private static IOrderedQueryable<Recipient> SortByName(IQueryable<Recipient> query, SortDirection direction)
        => direction == SortDirection.Desc
            ? query.OrderByDescending(recipient => recipient.Name).ThenBy(recipient => recipient.Id)
            : query.OrderBy(recipient => recipient.Name).ThenBy(recipient => recipient.Id);
}