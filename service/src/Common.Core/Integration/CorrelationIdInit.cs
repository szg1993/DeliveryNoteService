using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.Integration;

public static class CorrelationIdInit
{
    public static IServiceCollection AddMedusaCorrelationId(this IServiceCollection services)
        => services.AddScoped<CorrelationIdAccessor>();
}