using Common.Testing.Database;
using DeliveryNoteService.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        CommonStartup.ConfigureServices(services);

        services.AddSqlDatabase<AppDbContext>();
    }
}