using Medusa.Enap.Common.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

public class NavigationPropertyValidator(DbContext dbContext) : INavigationPropertyValidator
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsTracked<TModel>(TModel model)
        where TModel : class
    {
        var entry = dbContext.Entry(model);
        return entry.State != EntityState.Added && entry.State != EntityState.Detached;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsLoaded<TModel, TProp>(TModel model, Expression<Func<TModel, TProp?>> prop)
        where TProp : class
        where TModel : class
        => dbContext.Entry(model).Reference(prop).IsLoaded;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsCollectionLoaded<TModel, TProp>(TModel model, Expression<Func<TModel, IEnumerable<TProp>>> prop)
        where TProp : class
        where TModel : class
        => dbContext.Entry(model).Collection(prop).IsLoaded;
}