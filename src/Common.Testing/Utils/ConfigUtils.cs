using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Medusa.Enap.Common.Testing.Utils;

public static class ConfigUtils
{
    public static TOptions GetConfig<TOptions>(this IScenario scenario) where TOptions : class
        => scenario.Services.GetConfig<TOptions>();

    public static TOptions GetConfig<TOptions>(this IServiceProvider services) where TOptions : class
        => services.GetRequiredService<IOptions<TOptions>>().Value;

    public static void Config<TOptions>(this IScenario scenario, Action<TOptions> setter)
        where TOptions : class
    {
        var options = scenario.Services.GetRequiredService<IOptions<TOptions>>().Value;
        setter(options);
    }

    public static void Config<TOptions>(this IScenario scenario, string name, Action<TOptions> setter)
        where TOptions : class
    {
        var optionsMonitor = scenario.Services.GetRequiredService<IOptionsMonitor<TOptions>>();
        var namedOptions = optionsMonitor.Get(name);
        setter(namedOptions);
    }
}