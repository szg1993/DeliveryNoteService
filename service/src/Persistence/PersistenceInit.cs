using Common.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryNoteService.Persistence;

public static class PersistenceInit
{
    public static IServiceCollection AddDeliveryNoteServicePersistence(this IServiceCollection services, IConfigurationSection configuration)
    {
        return services.AddPersistenceCore<AppDbContext>(configuration);
    }

    public static Task InitDatabaseAsync(IServiceProvider services)
        => ModuleInit.InitDatabaseAsync(services);
}