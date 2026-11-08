using Medusa.Enap.Common.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.DemoService.Persistence;

public static class PersistenceInit
{
    public static IServiceCollection AddDemoServicePersistence(this IServiceCollection services, IConfigurationSection configuration)
    {
        return services.AddMedusaPersistenceCore<AppDbContext>(configuration);
    }

    public static Task InitDatabaseAsync(IServiceProvider services)
        => Common.Infrastructure.Persistence.ModuleInit.InitDatabaseAsync(services);
}
