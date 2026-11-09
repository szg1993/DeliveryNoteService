using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Common.AspNetCore;

public static class UseMetrics
{
    public static WebApplication UseMedusaMetrics(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();

        return app;
    }
}