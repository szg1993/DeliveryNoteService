using Medusa.Enap.Common.AspNetCore.Swagger.Annotations;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Utils;
using Microsoft.AspNetCore.Http;

namespace Medusa.Enap.Common.AspNetCore;

public class RequiredHeaderValidationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public Task Invoke(HttpContext context)
    {
        var requiredHeaders = context.GetEndpoint()?.Metadata
            .OfType<HeaderMetadata>()
            .Where(h => h.Required);

        if (requiredHeaders.IsNullOrEmpty())
            return _next(context);

        foreach (var requiredHeader in requiredHeaders!)
            ValidateRequiredHeader(context, requiredHeader.HeaderName);

        return _next(context);
    }

    private static void ValidateRequiredHeader(HttpContext context, string headerName)
    {
        var headerValue = context.Request.Headers[headerName].FirstOrDefault()?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(headerValue))
            throw new DataValidationException(CommonWebErrorCodes.MissingRequiredHeader, $"Header missing. ({headerName})")
                .SetData("Header", headerName);
    }
}
