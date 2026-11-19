using Common.Core.HealthProbes;
using Common.Core.Logging;

namespace DeliveryNoteService.WebApp.HealthProbes;

public static class AppInitializer
{
    public static async Task RunAsync(this WebApplication app, Func<WebApplication, Task> initServices, Func<WebApplication, Task>? cleanup = null)
    {
        await app.StartAsync();

        try
        {
            await initServices(app);

            app.Lifetime.ApplicationStopping.Register(() => cleanup?.Invoke(app).Wait());

            var readinessService = app.Services.GetRequiredService<IReadinessService>();
            readinessService.IsReady = true;
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<AppLogger>();
            logger.LogTechnical(LogLevel.Error, ex, "Application initialization failed");

            await app.StopAsync();
        }

        await app.WaitForShutdownAsync();
    }
}