using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ApiTests;

public class SingletonAsyncInitializer<T>(T fixture) : IHostedService
    where T : IAsyncLifetime
{
    public Task StartAsync(CancellationToken cancellationToken) => fixture.InitializeAsync();

    public Task StopAsync(CancellationToken cancellationToken) => fixture.DisposeAsync();
}

public static class SingletonAsyncInitializerHelper
{
    public static IServiceCollection AddAsyncInitializer<T>(this IServiceCollection services)
        where T : IAsyncLifetime
    {
        services.AddHostedService<SingletonAsyncInitializer<T>>();
        return services;
    }
}
