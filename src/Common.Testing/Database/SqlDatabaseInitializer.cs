using ApiTests;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.Database;

public static class SqlDatabaseInitializer
{
    public static IServiceCollection AddSqlDatabase<TDbContext>(this IServiceCollection services)
    {
        DatabaseFixture.DbContextType = typeof(TDbContext);
        services.AddSingleton<DatabaseFixture>();
        services.AddAsyncInitializer<DatabaseFixture>();
        return services;
    }
}
