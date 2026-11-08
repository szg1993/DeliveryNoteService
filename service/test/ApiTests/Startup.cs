using Medusa.Enap.Common.Testing.Database;
using Medusa.Enap.Common.Testing.MessageQueue.Fixtures;
using Medusa.Enap.DemoService.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        CommonStartup.ConfigureServices(services);

        services.AddSqlDatabase<AppDbContext>();

        services.AddSingleton<MessagingFixture>();
        services.AddAsyncInitializer<MessagingFixture>();
    }
}
