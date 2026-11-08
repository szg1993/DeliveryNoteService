using Medusa.Enap.Common.Core.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

public static class ModuleInit
{
    public static IServiceCollection AddMedusaPersistenceCore<TDbContext>(
        this IServiceCollection services,
        IConfigurationSection configuration
    )
        where TDbContext : DbContext, IUnitOfWork
    {
        services.AddDbContext<TDbContext>(opt =>
        {
            // connection string must be obtained within the lambda function
            // so that tests can overwrite the config value
            var connectionString = configuration.GetConnectionString("AppDb") ?? string.Empty;

            if (connectionString.IsNpgsql())
            {
                opt.UseNpgsql(connectionString);
            }
            else if (connectionString.IsSqlite())
            {
                opt.UseSqlite(connectionString);
                opt.AddHungarianCollation();
            }
            else
            {
                throw new NotSupportedException($"Given connection string {connectionString} is not supported");
            }
        });

        return services
            .AddSingleton<ICaseInsensitiveComparer, EfILikeComparer>()
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TDbContext>())
            // generic way of getting the application DbContext
            .AddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>())
            .AddRepositoriesForDbContext<TDbContext>()
            .AddScoped<INavigationPropertyValidator, NavigationPropertyValidator>()
            .AddScoped<NewEntityLogger>()
            ;
    }

    private static IServiceCollection AddRepositoriesForDbContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        // Get all DbSet properties of the DbContext
        var dbSets = DbSetInfo.Get<TContext>();

        foreach (var entityType in dbSets.Select(dbSet => dbSet.EntityType))
        {
            // Create the generic type for IRepository<> and Repository<>
            var repoInterface = typeof(IRepository<>).MakeGenericType(entityType);
            var repoImplementation = typeof(Repository<>).MakeGenericType(entityType);
            var readonlyInterface = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);
            var readonlyImplementation = typeof(ReadonlyRepository<>).MakeGenericType(entityType);

            // Register the services dynamically
            services.AddScoped(repoInterface, repoImplementation);
            services.AddScoped(readonlyInterface, readonlyImplementation);
        }

        return services;
    }

    public static async Task InitDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();

        db.Database.SetCommandTimeout(TimeSpan.FromHours(1));

        await db.Database.MigrateAsync();
    }

    public static bool IsNpgsql(this string connectionString)
    {
        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsSqlite(this string connectionString)
    {
        try
        {
            _ = new SqliteConnectionStringBuilder(connectionString);
            return true;
        }
        catch
        {

            return false;
        }
    }
}