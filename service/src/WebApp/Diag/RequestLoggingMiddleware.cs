using Common.Core.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace DeliveryNoteService.WebApp.Diag;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<DeliveryNoteServiceLogger<RequestLoggingMiddleware>>();
        var request = context.Request;
        var body = await ReadBody(request);
        logger.LogTechnical(LogLevel.Information, "Incoming request", new()
        {
            { "url", request.GetDisplayUrl() },
            { "method", request.Method },
            { "body", body }
        });

        await next(context);
    }

    private static async Task<string> ReadBody(HttpRequest request)
    {
        // to be able to read the body stream again down the pipeline
        // - enable request buffering
        // - leave the underlying stream open
        // - reset stream position to 0
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
}