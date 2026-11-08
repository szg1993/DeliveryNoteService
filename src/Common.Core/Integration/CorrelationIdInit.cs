using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Core.Integration;

public static class CorrelationIdInit
{
    public static IServiceCollection AddMedusaCorrelationId(this IServiceCollection services)
        => services.AddScoped<CorrelationIdAccessor>();
}
