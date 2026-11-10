using Common.Core.Exceptions;
using Common.Core.Messaging;
using Common.Core.Persistence;
using Common.Core.Utils;

namespace Common.Core.UseCases;

public static class PagedResultHelpers
{
    public static IQueryable<TModel> ApplyPaging<TModel>(this IQueryable<TModel> query, PageRequest pagination)
    {
        if (pagination.Limit == 0)
            return query;

        ValidatePageRequest(pagination);

        var offset = ((long)pagination.Page - 1) * pagination.Limit;
        if (offset > int.MaxValue)
            return query.Take(0);

        return query
            .Skip((int)offset)
            .Take(pagination.Limit);
    }

    public static Task<PagedEnvelope<TModel>> Paged<TModel>(
        this IReadOnlyRepository<TModel> repository,
        PageRequest pagination,
        Func<IQueryable<TModel>, IOrderedQueryable<TModel>> sorter,
        CancellationToken ct)
        where TModel : class
    {
        var sorted = repository.TransformQuery(sorter);
        return Paged(sorted, pagination, ct);
    }

    public static Task<PagedEnvelope<TModel>> Paged<TModel>(
        this IReadOnlyRepository<TModel> repository,
        PageRequest pagination,
        CancellationToken ct)
    {
        if (pagination.Limit == 0)
            return GetUnpaged(repository, ct);

        return GetPage(repository, pagination, ct);
    }

    private static async Task<PagedEnvelope<TModel>> GetUnpaged<TModel>(IReadOnlyRepository<TModel> repository, CancellationToken ct)
    {
        var m = await repository.GetAllAsync(ct);
        return new(m, PageRequest.Unpaged, m.Count());
    }

    private static async Task<PagedEnvelope<TModel>> GetPage<TModel>(IReadOnlyRepository<TModel> repository, PageRequest pagination, CancellationToken ct)
    {
        var totalCount = await repository.CountAsync(q => q, ct);

        var models = totalCount == 0 ? [] : await repository.GetAllAsync(q => q.ApplyPaging(pagination), ct);

        return new(models, pagination, totalCount);
    }

    public static Task<PagedEnvelope<T2>> As<T1, T2>(this Task<PagedEnvelope<T1>> originalTask, Func<T1, T2> transform)
        => originalTask.Then(original => original.As(transform));

    public static PagedEnvelope<T2> As<T1, T2>(this PagedEnvelope<T1> original, Func<T1, T2> transform)
        => new([.. original.Content.Select(transform)], original.Paging);

    private static void ValidatePageRequest(PageRequest param)
    {
        if (param.Page < 1)
            throw new DataValidationException(CommonErrorCodes.InvalidPageRequest)
                .SetData("page", param.Page);

        if (param.Limit < 1)
            throw new DataValidationException(CommonErrorCodes.InvalidPageRequest)
                .SetData("limit", param.Limit);
    }
}