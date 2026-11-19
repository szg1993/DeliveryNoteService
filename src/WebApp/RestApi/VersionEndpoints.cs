using Common.AspNetCore.Swagger.Annotations;
using Common.Core.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryNoteService.WebApp.RestApi;

public static class VersionEndpoints
{
    private const string _versions = "Versions";

    public static IEndpointRouteBuilder MapVersionsEndpoint(this IEndpointRouteBuilder app, string componentName)
    {
        app
            .MapGet(
                "/api/v1/Versions/{connectedServices}",
                (
                    [FromRoute] bool connectedServices,
                    GetVersionInfo getVersionInfo,
                    CancellationToken cancellationToken
                ) => getVersionInfo.RunAsync(componentName, connectedServices, cancellationToken))
            .WithTags(_versions)
            .WithDescription("Retrieves version information.")
            .Produces<VersionInfo>(StatusCodes.Status200OK)
            .ProducesBadRequest();

        return app;
    }
}