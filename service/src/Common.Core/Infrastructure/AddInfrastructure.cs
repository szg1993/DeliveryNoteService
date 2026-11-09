using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.Infrastructure;

public static class AddInfrastructure
{
    public static IServiceCollection AddMedusaInfrastructure(this IServiceCollection services)
        => services.AddSingleton<ITimeService, TimeService>();
}