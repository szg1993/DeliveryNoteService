using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using System.Data;
using System.Globalization;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

public class AppDbContextBase : DbContext, IUnitOfWork
{
    private readonly NewEntityLogger _logger;

    public AppDbContextBase(NewEntityLogger logger, DbContextOptions options) : base(options)
    {
        _logger = logger;
    }

    public Task<T> RunInTransactionAsync<T>(Func<Task<T>> function, CancellationToken ct)
        => RunInTransactionAsync(this, function, ct);

    public Task RunInTransactionAsync(Func<Task> function, CancellationToken ct)
        => RunInTransactionAsync(this, function, ct);

    /// <summary>
    /// This method needs attention and race condition testing! Its 100% POC yet! Here we are using a row lock on DB level instead of a custom made IsLocked column to prevent concurrent updates on a row.
    /// The goal of this function is to lock out any concurrent transaction on a given row.
    /// Using this method the followings will not be possible for concurrent transactions: Dirty Read, Nonrepeatable Read, Phantom Read, Serialization Anomaly.
    /// https://www.postgresql.org/docs/current/transaction-iso.html
    /// </summary>
    public Task RunInLockingTransactionAsync(Func<Task> function, CancellationToken ct)
        => RunInTransactionAsync(this, function, ct, IsolationLevel.Serializable);

    public void Reset()
        => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsSqlite())
            ConfigureSQLiteCompatibility(modelBuilder);

        if (Database.IsNpgsql())
            modelBuilder.AddHungarianCollation();

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetUtcConverter>();
    }

    public static void ConfigureSQLiteCompatibility(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Handle datetimes in SQLite src: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/

            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the ISO8601Converter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            var properties = entityType.ClrType.GetProperties()
                .Where(p =>
                    p.PropertyType == typeof(DateTimeOffset)
                 || p.PropertyType == typeof(DateTimeOffset?));
            foreach (var property in properties)
            {
                modelBuilder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new Iso8601Converter());
            }
        }
    }

    public static Task RunInTransactionAsync(DbContext dbContext, Func<Task> function, CancellationToken ct, IsolationLevel? isolationLevel = null)
        => RunInTransactionAsync(dbContext, async () => { await function(); return 0; }, ct, isolationLevel);

    public static async Task<T> RunInTransactionAsync<T>(DbContext dbContext, Func<Task<T>> function, CancellationToken ct, IsolationLevel? isolationLevel = null)
    {
        using var tr = isolationLevel is null
            ? await dbContext.Database.BeginTransactionAsync(ct)
            : await dbContext.Database.BeginTransactionAsync(isolationLevel.Value, ct);

        try
        {
            var res = await function();
            await dbContext.SaveChangesAsync(ct);
            await tr.CommitAsync(ct);
            return res;
        }
        catch
        {
            await tr.RollbackAsync(ct);
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Capture(ChangeTracker);
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger.Log();

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new UpdateConcurrencyException(ex);
        }
        catch (DbUpdateException ex)
        {
            if (IsPostgresUniqueKeyViolationException(ex) || IsSqliteUniqueKeyViolationException(ex))
                throw new UniqueKeyConflictException(ex);

            throw;
        }
    }

    private static bool IsPostgresUniqueKeyViolationException(Exception ex)
    {
        if (ex.InnerException is not PostgresException pgEx)
            return false;

        if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation) // 23505
            return true;

        return false;
    }

    private static bool IsSqliteUniqueKeyViolationException(Exception ex)
    {
        if (ex.InnerException is not SqliteException sqliteEx)
            return false;

        // SQLite unique violation
        if (sqliteEx.SqliteErrorCode == 19 &&
            sqliteEx.SqliteExtendedErrorCode == 2067)
        {
            return true;
        }

        return false;
    }
}

file sealed class DateTimeOffsetUtcConverter
    : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public DateTimeOffsetUtcConverter()
        : base(v => v.ToUniversalTime(), v => v)
    { }
}

file sealed class Iso8601Converter : ValueConverter<DateTimeOffset, string>
{
    public Iso8601Converter()
        : base(
            d => d.ToUniversalTime().ToString("o"),
            d => DateTimeOffset.Parse(d, CultureInfo.InvariantCulture))
    { }
}