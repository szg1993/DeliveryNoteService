using Medusa.Enap.Common.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Medusa.Enap.Common.Infrastructure.Scheduling;

public static class SchedulingInit
{
    /// <summary>
    /// Allows us to delay start the quartz schedulers after database has been migrated
    /// </summary>
    public static async Task StartSchedulers(IServiceProvider services)
    {
        var logger = services.GetRequiredService<AppLogger>();
        logger.LogTechnical(LogLevel.Information, "Starting Quartz scheduler");

        var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
        var s = await schedulerFactory.GetScheduler();
        if (!s.IsStarted && !s.IsShutdown)
            await s.Start();

        logger.LogTechnical(LogLevel.Information, "Started Quartz scheduler " + s.SchedulerName);
    }

    public static async Task StopSchedulers(IServiceProvider services)
    {
        var logger = services.GetRequiredService<AppLogger>();
        logger.LogTechnical(LogLevel.Information, "Stopping Quartz scheduler");

        var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
        var s = await schedulerFactory.GetScheduler();
        await s.Shutdown(waitForJobsToComplete: true);

        logger.LogTechnical(LogLevel.Information, "Stopped Quartz scheduler " + s.SchedulerName);
    }
}
