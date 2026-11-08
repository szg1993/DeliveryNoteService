using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Medusa.Enap.Common.AspNetCore;

public static class UseMetrics
{
    public static WebApplication UseMedusaMetrics(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();

        return app;
    }
}