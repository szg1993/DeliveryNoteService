using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Medusa.Enap.Common.Testing.TestSuite;

internal class MedusaWebAppFactory<TWebAppEntryPoint> : WebApplicationFactory<TWebAppEntryPoint>
    where TWebAppEntryPoint : class
{
    private Action<IConfigurationBuilder> _configOverride = _ => { };

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => _configOverride(config));
        return base.CreateHost(builder);
    }

    internal MedusaWebAppFactory<TWebAppEntryPoint> ConfigureHostConfiguration(Action<IConfigurationBuilder> configOverride)
    {
        _configOverride = configOverride;
        return this;
    }
}
