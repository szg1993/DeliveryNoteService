using Common.Core.Integration;
using Common.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Common.AspNetCore.Correlation;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        CorrelationIdAccessor correlationIdAccessor,
        MedusaLogger<CorrelationIdMiddleware> logger)
    {
        context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId);
        correlationIdAccessor.CorrelationId = correlationId!;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append("X-Correlation-ID", correlationIdAccessor.CorrelationId);
            return Task.CompletedTask;
        });

        using (logger.PushCorrelationIdProperty(correlationIdAccessor.CorrelationId))
            await next(context);
    }
}