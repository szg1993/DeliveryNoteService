using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.Integration;

public static class CorrelationIdInit
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
        => services.AddScoped<CorrelationIdAccessor>();
}