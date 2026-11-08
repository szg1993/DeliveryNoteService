using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Core.Versioning;

public static class VersioningInit
{
    public static IServiceCollection AddMedusaVersioning(this IServiceCollection services, IConfigurationSection config)
    {
        return services
            .AddScoped<GetVersionInfo>()
            .AddSingleton<ILocalVersionService, LocalVersionService>()
            .AddOptions<VersionSettings>().Bind(config).Services
            ;
    }
}
