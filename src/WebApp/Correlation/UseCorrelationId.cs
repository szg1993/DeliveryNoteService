namespace DeliveryNoteService.WebApp.Correlation;

public static class UseCorrelationId
{
    public static IApplicationBuilder UseMedusaCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}