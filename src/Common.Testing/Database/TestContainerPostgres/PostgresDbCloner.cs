using Npgsql;

namespace Common.Testing.Database.TestContainerPostgres;

internal static class PostgresDbCloner
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public static async Task CloneDatabaseAsync(
        string originalConnectionString,
        string newDatabaseName,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var adminConn = await OpenAdminConnection(originalConnectionString, cancellationToken);
            // Important: no connections should be active to the template DB at this point,
            // otherwise this will fail with "database ... is being accessed by other users".
            await CloneDb(new NpgsqlConnectionStringBuilder(originalConnectionString).Database!, newDatabaseName, adminConn, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static async Task<NpgsqlConnection> OpenAdminConnection(string originalConnectionString, CancellationToken cancellationToken)
    {
        var adminBuilder = new NpgsqlConnectionStringBuilder(originalConnectionString)
        {
            Database = "postgres",
            // only 1 simultaneous connection is allowed, but let's reuse the connection
            // because each test starts with cloning the template DB
            Pooling = true,
            MaxPoolSize = 1
        };
        var adminConn = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConn.OpenAsync(cancellationToken);
        return adminConn;
    }

    private static async Task CloneDb(string templateDbName, string clonedDbName, NpgsqlConnection adminConn, CancellationToken cancellationToken)
    {
        await using var createCmd = adminConn.CreateCommand();

        createCmd.CommandText =
            $"CREATE DATABASE {QuoteIdent(clonedDbName)} TEMPLATE {QuoteIdent(templateDbName)};";
        await createCmd.ExecuteNonQueryAsync(cancellationToken);
    }
    private static string QuoteIdent(string ident)
    {
        // Basic identifier quoting for safety
        return "\"" + ident.Replace("\"", "\"\"") + "\"";
    }
}