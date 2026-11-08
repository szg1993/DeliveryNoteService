using Medallion.Threading.Postgres;
using Medallion.Threading;
using Medusa.Enap.Common.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Microsoft.EntityFrameworkCore;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Infrastructure.Persistence;
using Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;

namespace Medusa.Enap.Common.Infrastructure.Scheduling;

public class ScheduleSettingsBase
{
    public bool DisableStartupJobs { get; set; } = false;
}

public static class AddScheduling
{
    public static IServiceCollection AddMedusaScheduling(
        this IServiceCollection services,
        string connectionString,
        Action<IServiceCollectionQuartzConfigurator> config
    )
    {
        return services
            .AddQuartz(config)
            .AddScoped<LocalMedusaJobContext>()
            .AddScoped<ClusteredMedusaJobContext>()
            .AddScoped<PersistentJobExecutionLedger>()
            .AddScoped<IRepository<JobExecution>, Repository<JobExecution>>()
            .AddSingleton<IDistributedLockProvider>((_) => new PostgresDistributedSynchronizationProvider(connectionString))
            .AddKeyedSingleton<ILockProvider, LocalLockProvider>(LocalLockProvider.Key)
            .AddKeyedSingleton<ILockProvider, ClusteredLockProvider>(ClusteredLockProvider.Key)
            ;
    }

    public static ModelBuilder AddJobExecutionLedger(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobExecution>(b =>
        {
            b.ToTable("JobExecutions");
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.JobName, m.ScheduledFireTime });
        });

        return modelBuilder;
    }

    public static IServiceCollectionQuartzConfigurator AddMedusaJob<TJob>(
        this IServiceCollectionQuartzConfigurator config,
        IServiceCollection services,
        string cronSchedule,
        bool startImmediately = false,
        ScheduleSettingsBase? settings = null
    )
        where TJob : MedusaJob
    {

        services.AddScoped<TJob>();

        var jobKey = new JobKey(typeof(TJob).Name);

        config
            .AddJob<TJob>(jc => jc.WithIdentity(jobKey))
            .AddTrigger(tc => tc
                .ForJob(jobKey)
                .WithCronSchedule(cronSchedule)
                .StartNow()
            );

        if (startImmediately && settings?.DisableStartupJobs != true)
            config.AddTrigger(tc => tc
                .ForJob(jobKey)
                .StartNow()
                .WithSimpleSchedule(s => s.WithRepeatCount(0))
            );

        return config;
    }
}
