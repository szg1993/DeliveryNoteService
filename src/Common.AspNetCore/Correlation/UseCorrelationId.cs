using Microsoft.AspNetCore.Builder;

namespace Medusa.Enap.Common.AspNetCore.Correlation;
public static class UseCorrelationId
{
    public static IApplicationBuilder UseMedusaCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
