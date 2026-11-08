using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Persistence;

public interface IReadOnlyRepository<TModel>
{
    #region Repository Modifiers

    IReadOnlyRepository<TModel> NoTracking();

    IReadOnlyRepository<TModel> Include(Expression<Func<TModel, object?>> include);

    IReadOnlyRepository<TModel> Include<TProp>(
        Expression<Func<TModel, TProp>> include,
        Expression<Func<TProp, object?>> thenInclude);

    IReadOnlyRepository<TModel> Include<TProp>(
        Expression<Func<TModel, ICollection<TProp>>> include,
        Expression<Func<TProp, object?>> thenInclude);

    IReadOnlyRepository<TModel> Include<TProp1, TProp2>(
        Expression<Func<TModel, ICollection<TProp1>>> include,
        Expression<Func<TProp1, ICollection<TProp2>>> thenInclude1,
        Expression<Func<TProp2, object?>> thenInclude2);

    IReadOnlyRepository<T> TransformModel<T>(Expression<Func<TModel, T>> transform) where T : class;

    IReadOnlyRepository<T> TransformQuery<T>(Func<IQueryable<TModel>, IQueryable<T>> transform) where T : class;

    #endregion

    IQueryable<TModel> QueryAll();

    Task<int> CountAsync(Func<IQueryable<TModel>, IQueryable<TModel>> filter, CancellationToken cancellationToken);

    Task<TResult?> MaxAsync<TResult>(Expression<Func<TModel, TResult?>> selector, CancellationToken cancellationToken)
        where TResult : struct;

    Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken);

    Task<bool> AnyAsync(Func<IQueryable<TModel>, IQueryable<TModel>> query, CancellationToken cancellationToken);

    Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TModel>, IQueryable<TResult>> query, CancellationToken cancellationToken);

    Task<TResult?> FindAsync<TResult>(Func<IQueryable<TModel>, IQueryable<TResult>> query, CancellationToken cancellationToken);

    bool IsLoaded<TProp>(TModel model, Expression<Func<TModel, TProp?>> prop) where TProp : class;

    bool IsLoaded<TProp>(TModel model, Expression<Func<TModel, IEnumerable<TProp>>> prop) where TProp : class;
}
