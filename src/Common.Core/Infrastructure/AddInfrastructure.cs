using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.Infrastructure;

public static class InfrastructureInit
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services.AddSingleton<ITimeService, TimeService>();
}