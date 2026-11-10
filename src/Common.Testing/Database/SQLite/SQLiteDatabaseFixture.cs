using Common.Core.Model;
using Common.Core.Persistence;
using Common.Core.Utils;
using Common.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Common.Testing.Database.SQLite;

public sealed class SQLiteDatabaseFixture(Type dbContextType) : IDatabaseFixture
{
    private const string _dbFolder = "DbFiles";
    private readonly ConcurrentDictionary<string, string> _dbFiles = [];
    private static byte[]? _templateDbBinary = null;

    public NullSortingStyle NullSortingStyle => NullSortingStyle.NullFirst;

    public bool SupportsCluster => false;

    #region Global Setup/TearDown

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_dbFolder);
        Directory.GetFiles(_dbFolder)
            .ForEach(File.Delete);

        var templateDbFileName = Path.Combine(_dbFolder, $"seeded.db");

        await CreateDataBaseAsync(templateDbFileName, dbContextType);

        _templateDbBinary = await File.ReadAllBytesAsync(templateDbFileName);
        File.Delete(templateDbFileName);
    }

    public async Task DisposeAsync()
    {
        foreach (var dbFile in _dbFiles.Values)
            await SQLiteUtils.DeleteAsync(dbFile);
    }

    #endregion

    #region Per Test Setup/TearDown

    public async Task<string> CreateTestDbAsync(string testId)
    {
        var testDbFileName = CreateDbFilePath(testId);
        if (_dbFiles.TryAdd(testId, testDbFileName))
            await File.WriteAllBytesAsync(testDbFileName, _templateDbBinary!);

        return SQLiteUtils.CreateConnectionString(CreateDbFilePath(testId));
    }

    public async Task ClearTestDbAsync(string testId)
    {
        if (_dbFiles.TryRemove(testId, out var dbFile))
            await SQLiteUtils.DeleteAsync(dbFile);
    }

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ICaseInsensitiveComparer, InMemoryEfILikeComparer>();
    }

    #endregion

    private static async Task CreateDataBaseAsync(string dbFileName, Type dbContextType)
    {
        var conn = new SqliteConnection(SQLiteUtils.CreateConnectionString(dbFileName));
        conn.AddHungarianCollation();
        var opts = new DbContextOptionsBuilder().UseSqlite(conn).Options;
        await using var ctx = Activator.CreateInstance(dbContextType, opts) as DbContext;

        await ctx!.Database.MigrateAsync();
        await ConfigureSequencesAsync(ctx);

        SqliteConnection.ClearPool(conn);
        await conn.CloseAsync();
    }

    private static async Task ConfigureSequencesAsync(DbContext context)
    {
        var dbSets = DbSetInfo.Get(context)
            .Where(dbSet => dbSet.EntityType.IsAssignableTo(typeof(IHaveId)));

        var i = 1;

        // NOTE: this command enables writing the table schema by updating the create sql command stored in the 'sqlite_master' table
        await context.Database.ExecuteSqlRawAsync("PRAGMA writable_schema=ON;");

        foreach (var dbSet in dbSets)
        {
            var updateSchemaSql = AddAutoIncrementToSchema(dbSet.Name);

            await context.Database.ExecuteSqlRawAsync(updateSchemaSql);
            await context.Database.ExecuteSqlAsync($"INSERT INTO sqlite_sequence (name, seq) SELECT '{dbSet.Name}', 1 WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name='{dbSet.Name}');");
            await context.Database.ExecuteSqlAsync($"UPDATE sqlite_sequence SET seq={i++ * 100} WHERE name='{dbSet.Name}';");
        }

        // NOTE: this command applies the schema changes to the database, and turns writable_schema off
        await context.Database.ExecuteSqlRawAsync("PRAGMA writable_schema=RESET;");
    }

    /// <summary>
    /// This method adds the AUTOINCREMENT annotation (if it is missing) to the primary key fields in Sqlite.
    /// This is needed because sequence values stored in the 'sqlite_sequence' table are ignored when AUTOINCREMENT is not set.
    /// EF Core may not add AUTOINCREMENT automatically if an Sqlite database is created using migrations added with a provider other than Sqlite.
    /// </summary>
    /// <param name="tableName">The name of the Sqlite DB table</param>
    /// <returns>The UPDATE sql command as string</returns>
    private static string AddAutoIncrementToSchema(string tableName)
        => $"""
              CREATE TABLE IF NOT EXISTS tmptable (Id INTEGER PRIMARY KEY AUTOINCREMENT);
              DROP TABLE tmptable;

              UPDATE sqlite_master 
                SET sql=SUBSTR(sql, 1, INSTR(sql, ',') - 1) || ' AUTOINCREMENT,' || SUBSTR(sql, INSTR(sql, ',') + 1) 
                WHERE name='{tableName}' 
                AND INSTR(sql, 'AUTOINCREMENT') = 0 
                AND INSTR(sql, ',') > 0;
            """;

    private static string CreateDbFilePath(string testId)
    {
        return Path.Combine(_dbFolder, $"{testId}.db");
    }
}