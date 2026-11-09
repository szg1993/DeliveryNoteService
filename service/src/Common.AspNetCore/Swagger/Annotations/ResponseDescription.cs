using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.AspNetCore.Swagger.Annotations;

public static class ResponseDescriptionUtil
{
    public static RouteHandlerBuilder WithResponseDescription(this RouteHandlerBuilder builder, int statusCode, string? description)
        => description is null ? builder
            : builder.WithMetadata(new ResponseDescriptionMetadata(statusCode, description));
}

internal sealed record ResponseDescriptionMetadata(int StatusCode, string Description);

internal sealed class ResponseDescriptionFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        foreach (var m in endpointMetadata.OfType<ResponseDescriptionMetadata>())
        {
            var key = m.StatusCode.ToString();
            if (operation.Responses.TryGetValue(key, out var response))
                response.Description = m.Description;
        }
    }
}