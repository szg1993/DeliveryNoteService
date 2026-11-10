using Common.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Common.Infrastructure.Persistence;

public class ReadonlyRepository<TModel>(DbContext dbContext, IQueryable<TModel> q) : IReadOnlyRepository<TModel>
    where TModel : class
{
    public ReadonlyRepository(DbContext dbContext) : this(dbContext, dbContext.Set<TModel>())
    { }

    #region Repository Modifiers

    public IReadOnlyRepository<TModel> NoTracking()
        => TransformQuery(q => q.AsNoTracking());

    public IReadOnlyRepository<TModel> Include(Expression<Func<TModel, object?>> include)
        => TransformQuery(q => q.Include(include));

    public IReadOnlyRepository<TModel> Include<TProp>(
        Expression<Func<TModel, TProp>> include,
        Expression<Func<TProp, object?>> thenInclude
    ) => TransformQuery(q => q.Include(include).ThenInclude(thenInclude));

    public IReadOnlyRepository<TModel> Include<TProp>(
        Expression<Func<TModel, ICollection<TProp>>> include,
        Expression<Func<TProp, object?>> thenInclude
    ) => TransformQuery(q => q.Include(include).ThenInclude(thenInclude));

    public IReadOnlyRepository<TModel> Include<TProp1, TProp2>(
        Expression<Func<TModel, ICollection<TProp1>>> include,
        Expression<Func<TProp1, ICollection<TProp2>>> thenInclude1,
        Expression<Func<TProp2, object?>> thenInclude2
    ) => TransformQuery(q => q.Include(include).ThenInclude(thenInclude1).ThenInclude(thenInclude2));

    public IReadOnlyRepository<T> TransformModel<T>(Expression<Func<TModel, T>> transform) where T : class
        => new ReadonlyRepository<T>(dbContext, q.Select(transform));

    public IReadOnlyRepository<T> TransformQuery<T>(Func<IQueryable<TModel>, IQueryable<T>> transform) where T : class
        => new ReadonlyRepository<T>(dbContext, transform(q));

    #endregion

    public IQueryable<TModel> QueryAll() => q;

    public Task<bool> AnyAsync(Expression<Func<TModel, bool>> predicate, CancellationToken cancellationToken)
        => q.AnyAsync(predicate, cancellationToken);

    public Task<bool> AnyAsync(Func<IQueryable<TModel>, IQueryable<TModel>> query, CancellationToken cancellationToken)
        => query(q).AnyAsync(cancellationToken);

    public Task<int> CountAsync(Func<IQueryable<TModel>, IQueryable<TModel>> filter, CancellationToken cancellationToken)
        => filter(q).CountAsync(cancellationToken);

    public Task<TResult?> MaxAsync<TResult>(Expression<Func<TModel, TResult?>> selector, CancellationToken cancellationToken)
        where TResult : struct
        => q.MaxAsync(selector, cancellationToken);

    public Task<TResult?> FindAsync<TResult>(Func<IQueryable<TModel>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        => query(q).SingleOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Func<IQueryable<TModel>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        => await query(q).ToArrayAsync(cancellationToken);

    public bool IsLoaded<TProp>(TModel model, Expression<Func<TModel, TProp?>> prop)
        where TProp : class
        => new NavigationPropertyValidator(dbContext).IsLoaded(model, prop);

    public bool IsLoaded<TProp>(TModel model, Expression<Func<TModel, IEnumerable<TProp>>> prop) where TProp : class
        => new NavigationPropertyValidator(dbContext).IsCollectionLoaded(model, prop);
}