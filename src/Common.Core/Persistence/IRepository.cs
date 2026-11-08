using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Persistence;

public interface IRepository<TModel> : IReadOnlyRepository<TModel>
{
    TModel Add(TModel model);

    void AddRange(IEnumerable<TModel> models);

    void Delete(TModel model);

    void DeleteRange(IEnumerable<TModel> models);

    Task LoadAsync<TProp>(IEnumerable<TModel> models, Expression<Func<TModel, TProp?>> include, CancellationToken cancellationToken) where TProp : class;

    Task LoadCollectionAsync<TProp>(IEnumerable<TModel> models, Expression<Func<TModel, IEnumerable<TProp>>> include, CancellationToken cancellationToken) where TProp : class;
}
