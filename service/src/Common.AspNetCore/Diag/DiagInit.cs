using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Common.AspNetCore.Diag;

public static class DiagInit
{
    public static IApplicationBuilder UseMedusaDiag(this IApplicationBuilder app, IConfiguration config)
    {
        if (config.Get<DiagSettings>()?.RequestLoggingEnabled ?? false)
            app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}