using Common.Core.Logging;
using Common.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace Common.Infrastructure.Persistence;

public class NewEntityLogger(DeliveryNoteServiceLogger<NewEntityLogger> logger)
{
    private List<IModel> _added = [];

    public virtual void Capture(ChangeTracker changeTracker)
    {
        _added = changeTracker
            .Entries<IModel>()
            .Where(e => e.State is EntityState.Added).Select(e => e.Entity)
            .ToList();
    }

    public virtual void Log()
    {
        _added.ForEach(e => logger.LogTechnical(Microsoft.Extensions.Logging.LogLevel.Information, $"New {e.GetType().Name} has been saved. Id: {e.Id}"));
        _added.Clear();
    }
}