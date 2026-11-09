using Common.AspNetCore.Swagger.Annotations;
using Common.Core.HealthProbes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Common.AspNetCore.HealthProbes;

public static class HealthEndpoints
{
    private const string _health = "Health";

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app
            .MapGet("/health/liveness", () => Results.Ok("healthy"))
            .WithTags(_health)
            .ProducesOk<string>("Returns liveness state");

        app
            .MapGet(
                "/health/readiness",
                (
                    [FromServices] IReadinessService readinessService
                ) => readinessService.IsReady
                    ? Results.Ok("ready")
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
            )
            .WithTags(_health)
            .ProducesOk<string>("Returns readiness state")
            .Produces(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}