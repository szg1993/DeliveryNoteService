using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data.Common;
using System.Globalization;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

/// <summary>
/// Contains utilities so that Hungarian letters (e.g.: á, é) will be properly
/// ordered by Postgres and SQLite databases, instead of ordering them after the
/// English alphabet letters.
/// </summary>
public static class CollationUtils
{
    private const string _collationName = "hun_collation";

    /// <summary>
    /// Registers Hungarian Collation centrally in the model. It works for Postgres but not with SQLite.
    /// </summary>
    public static ModelBuilder AddHungarianCollation(this ModelBuilder modelBuilder)
    {
        return modelBuilder
            .HasCollation(
                name: _collationName,
                locale: "hu-HU",
                provider: "icu",
                deterministic: true
            );
    }

    /// <summary>
    /// Registers a DbConnectionInterceptor that adds Hungarian Collation to
    /// all SQLite connections
    /// </summary>
    public static void AddHungarianCollation(this DbContextOptionsBuilder opt)
        => opt.AddInterceptors(new SqliteHungarianCollationInterceptor());

    /// <summary>
    /// Adds Hungarian Collation to a single SQLite connection
    /// </summary>
    public static void AddHungarianCollation(this DbConnection connection)
    {
        if (connection is not SqliteConnection sqlite)
            return;

        var hu = CultureInfo.GetCultureInfo("hu-HU");

        sqlite.CreateCollation(
            _collationName,
            (x, y) => string.Compare(x, y, hu, CompareOptions.None));
    }

    /// <summary>
    /// Modifies a column definition to use Hungarian Collation
    /// </summary>
    public static PropertyBuilder UseHungarianCollation(this PropertyBuilder propertyBuilder)
    {
        return propertyBuilder.UseCollation(_collationName);
    }
}

file sealed class SqliteHungarianCollationInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        CollationUtils.AddHungarianCollation(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        CollationUtils.AddHungarianCollation(connection);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }
}