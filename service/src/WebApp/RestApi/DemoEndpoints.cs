using Medusa.Enap.Common.AspNetCore.Swagger.Annotations;
using Medusa.Enap.Common.AspNetCore.WebApi;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.DemoService.Application;
using Microsoft.AspNetCore.Mvc;

namespace Medusa.Enap.DemoService.WebApp.RestApi;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/demo-models/{id}",
            async (
                [FromRoute] int id,
                [FromServices] GetDemoModel getDemoModel,
                CancellationToken ct
            ) =>
            {
                return await getDemoModel.RunAsync(id, ct);
            })
            .WithTags("Demo");

        app.MapGet("/api/demo-models",
            async (
                [AsParameters] PageRequestQueryParam pageRequest,
                [AsParameters] Sorting<DemoModelsSortBy> sorting,
                [FromServices] GetDemoModels getDemoModels,
                CancellationToken ct
            ) =>
            {
                return await getDemoModels.RunAsync(pageRequest.ToUseCaseObject(), sorting, ct);
            })
            .WithTags("Demo");

        app.MapPost("/api/demo-rpc-endpoint",
            (
                [FromHeader(Name = "X-Demo-Header-Parameter")] string demoHeader
            ) =>
            {
                return Results.NoContent();
            })
            .WithTags("Demo")
            .RequireHeader("X-Demo-Required-Header");

        return app;
    }
}
