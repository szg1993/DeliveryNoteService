namespace DeliveryNoteService.WebApp.Diag;

public static class DiagInit
{
    public static IApplicationBuilder UseMedusaDiag(this IApplicationBuilder app, IConfiguration config)
    {
        if (config.Get<DiagSettings>()?.RequestLoggingEnabled ?? false)
            app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
