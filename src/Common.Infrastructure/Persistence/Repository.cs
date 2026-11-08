using Medusa.Enap.Common.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

public class Repository<TModel> : ReadonlyRepository<TModel>, IRepository<TModel> where TModel : class
{
    private readonly DbSet<TModel> _dbSet;
    private readonly DbContext _dbContext;

    public Repository(DbContext dbContext) : base(dbContext, dbContext.Set<TModel>())
    {
        _dbSet = dbContext.Set<TModel>();
        _dbContext = dbContext;
    }

    public TModel Add(TModel model)
        => _dbSet.Add(model).Entity;

    public void AddRange(IEnumerable<TModel> models)
        => _dbSet.AddRange(models);

    public void Delete(TModel model)
        => _dbSet.Remove(model);

    public void DeleteRange(IEnumerable<TModel> models)
        => _dbSet.RemoveRange(models);

    public async Task LoadAsync<TProp>(IEnumerable<TModel> models, Expression<Func<TModel, TProp?>> include, CancellationToken cancellationToken)
        where TProp : class
    {
        // For some reason the loaded references may miss identity resolution and multiple tracked instances are created
        // for the same loaded entity.
        // Just calling ChangeTracker.Entries() (to see if the models are properly tracked) fixes it, so leaving it here for now.
        _dbContext.ChangeTracker.Entries();
        foreach (var m in models)
            await _dbSet.Entry(m).Reference(include).LoadAsync(LoadOptions.ForceIdentityResolution, cancellationToken);
    }

    public async Task LoadCollectionAsync<TProp>(IEnumerable<TModel> models, Expression<Func<TModel, IEnumerable<TProp>>> include, CancellationToken cancellationToken)
        where TProp : class
    {
        _dbContext.ChangeTracker.Entries();
        foreach (var m in models)
            await _dbSet.Entry(m).Collection(include).LoadAsync(LoadOptions.ForceIdentityResolution, cancellationToken);
    }
}
