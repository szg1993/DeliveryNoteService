using Microsoft.AspNetCore.Builder;

namespace Common.AspNetCore.Correlation;

public static class UseCorrelationId
{
    public static IApplicationBuilder UseMedusaCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}