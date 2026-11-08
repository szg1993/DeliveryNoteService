using Microsoft.Data.Sqlite;

namespace Medusa.Enap.Common.Testing.Database.SQLite;

public static class SQLiteUtils
{
    public static string CreateConnectionString(string dbFileName)
    {
        return "Data Source=" + dbFileName;
    }

    public static async Task DeleteAsync(string dbFile)
    {
        // NOTE: these commands clear the connection pool associated with the db. We should not have any open connections when we try to delete the database file.
        using (SqliteConnection connection = new(CreateConnectionString(dbFile)))
        {
            SqliteConnection.ClearPool(connection);
            await connection.CloseAsync();
        }

        File.Delete(dbFile);
    }
}
