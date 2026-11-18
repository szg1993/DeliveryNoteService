using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Infrastructure.Persistence;

public static class PendingModelChangesExtensions
{
    /// <summary>
    /// Keeps the EF Core 8 behavior of applying migrations when the snapshot differs from the model.
    /// The migrations are generated for PostgreSQL but are also applied to SQLite, so EF Core 9+
    /// always sees provider differences as pending changes and would refuse to migrate.
    /// </summary>
    public static DbContextOptionsBuilder IgnorePendingModelChanges(this DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
}