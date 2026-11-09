using Microsoft.EntityFrameworkCore;

namespace Common.Testing.Database.TestContainerPostgres;

internal static class PostgresTemplateDbInitializer
{
    public static async Task CreateTemplateDbAsync(string connectionStringTemplate, Type dbContextType)
    {
        var opts = new DbContextOptionsBuilder().UseNpgsql(connectionStringTemplate).Options;
        await using var ctx = Activator.CreateInstance(dbContextType, opts) as DbContext;

        await ctx!.Database.MigrateAsync();
        await ctx.Database.ExecuteSqlRawAsync(_alterSequencesScript);
    }

    private const string _alterSequencesScript = """
                DO $$
        DECLARE
            seq RECORD;
            iteration INT := 1; -- Start iteration counter at 1
            start_value BIGINT;
        BEGIN
            -- Loop through all sequences in the public schema
            FOR seq IN
                SELECT c.relname AS seq_name
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE c.relkind = 'S'  -- 'S' = sequence
                  AND n.nspname = 'public'
                ORDER BY c.relname
            LOOP
                -- Calculate start value for this sequence
                start_value := iteration * 100;

                -- Build and execute ALTER SEQUENCE command
                EXECUTE format(
                    'ALTER SEQUENCE public.%I RESTART WITH %s',
                    seq.seq_name,
                    start_value
                );

                RAISE NOTICE 'Sequence % restarted with %', seq.seq_name, start_value;

                -- Increment iteration counter
                iteration := iteration + 1;
            END LOOP;
        END
        $$ LANGUAGE plpgsql;
        """;

}