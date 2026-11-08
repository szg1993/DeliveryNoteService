using Medusa.Enap.Common.Infrastructure.Scheduling;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public static class AddScheduling
{
    public static IServiceCollection AddDemoServiceScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Scheduling").Get<ScheduleSettings>()
            ?? throw new InvalidOperationException("Cannot read scheduling configuration.");

        var connectionString = configuration.GetSection("Persistence").GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("AppDb connection string not found");

        return services
            .AddMedusaScheduling(connectionString, config =>
            {
                config
                    .AddMedusaJob<LocalDemoJob>(services, settings.DemoCron, startImmediately: true, settings)
                    .AddMedusaJob<ClusteredDemoJob>(services, settings.DemoCron, startImmediately: true, settings)
                    ;
            });
    }
}
