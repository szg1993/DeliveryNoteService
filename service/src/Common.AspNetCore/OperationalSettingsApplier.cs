using Common.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.AspNetCore;

public static class OperationalSettingsApplier
{
    public static IServiceCollection ApplyMedusaOperationalSettings(this IServiceCollection services, IConfigurationSection configuration)
    {
        return services
            .Configure<HostOptions>(options =>
            {
                var timeoutStr = configuration?["GracefulShutdownTimeout"];
                if (!string.IsNullOrEmpty(timeoutStr))
                {
                    options.ShutdownTimeout = DurationParser.Parse(timeoutStr);
                }
            })
            ;
    }
}