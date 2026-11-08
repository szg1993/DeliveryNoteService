using Medusa.Enap.Common.AspNetCore.Swagger.Annotations;
using Medusa.Enap.Common.Core.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Medusa.Enap.Common.AspNetCore;

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
