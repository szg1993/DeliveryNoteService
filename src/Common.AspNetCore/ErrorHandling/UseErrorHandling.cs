using Microsoft.AspNetCore.Builder;

namespace Common.AspNetCore.ErrorHandling;

public static class UseErrorHandling
{
    public static IApplicationBuilder UseMedusaErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlingMiddleware>();
}