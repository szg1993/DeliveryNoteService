using Common.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Common.AspNetCore.Logging;

public static class AddLogging
{
    public static IServiceCollection AddMedusaLogging(this IServiceCollection services,
        string moduleName, IConfigurationSection? config = null)
    {
        return services
            .AddTransient(typeof(MedusaLogger<>))
            .AddTransient(typeof(BusinessEventLogger<>))
            .AddTransient(typeof(TechnicalUseCaseLogger<>))
            .AddHttpContextAccessor()
            .AddSerilog((sp, serilogConfig) =>
            {
                var rootConfig = sp.GetRequiredService<IConfiguration>();
                serilogConfig
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Module", moduleName)
                    .WriteTo.Console(new MedusaLogFormatter())
                    .WriteToSeq(config)
                    .CopyMicrosoftMinimumLogLevels(rootConfig)
                    // overwrite any hardcoded config from appsettings.json
                    .ReadFrom.Configuration(rootConfig);
            });
    }

    public static LoggerConfiguration CopyMicrosoftMinimumLogLevels(this LoggerConfiguration c, IConfiguration configuration)
    {
        var cfgLogLevel = configuration.GetSection("Logging:LogLevel");
        foreach (var configItem in cfgLogLevel.GetChildren())
        {
            var logLevel = Enum.TryParse(configItem.Value, out LogEventLevel l) ? l : LogEventLevel.Information;
            if (configItem.Key == "Default")
                c.MinimumLevel.Is(logLevel);
            else
                c.MinimumLevel.Override(configItem.Key, logLevel);
        }
        return c;
    }
}