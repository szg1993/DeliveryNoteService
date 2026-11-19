using Common.Core.HealthProbes;

namespace DeliveryNoteService.WebApp.HealthProbes;

public static class HealthProbesInit
{
    public static IServiceCollection AddHealthProbes(this IServiceCollection services)
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