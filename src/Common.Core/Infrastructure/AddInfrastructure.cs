using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Core.Infrastructure;

public static class AddInfrastructure
{
    public static IServiceCollection AddMedusaInfrastructure(this IServiceCollection services)
        => services.AddSingleton<ITimeService, TimeService>();
}