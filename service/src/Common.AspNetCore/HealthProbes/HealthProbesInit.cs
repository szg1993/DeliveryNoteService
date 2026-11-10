using Common.Core.HealthProbes;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.AspNetCore.HealthProbes;

public static class HealthProbesInit
{
    public static IServiceCollection AddMedusaHealthProbes(this IServiceCollection services)
    {
        return services.AddSingleton<IReadinessService, ReadinessService>();
    }

    public static WebApplication UseMedusaHealthProbes(this WebApplication app)
    {
        app.MapHealthEndpoints();

        app.Lifetime.ApplicationStopping.Register(
            () =>
            {
                var readinessService = app.Services.GetRequiredService<IReadinessService>();
                readinessService.IsReady = false;
            }
        );

        return app;
    }
}