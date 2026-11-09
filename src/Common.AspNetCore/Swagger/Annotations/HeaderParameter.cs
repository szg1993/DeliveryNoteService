using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.AspNetCore.Swagger.Annotations;

public static class HeaderMetadataUtil
{
    public static RouteHandlerBuilder WithOptionalHeader(this RouteHandlerBuilder builder, string headerName, string type = "string")
        => builder.WithMetadata(new HeaderMetadata(headerName, type, Required: false));

    public static RouteHandlerBuilder RequireHeader(this RouteHandlerBuilder builder, string headerName, string type = "string")
        => builder.WithMetadata(new HeaderMetadata(headerName, type, Required: true));
}

internal record HeaderMetadata(string HeaderName, string Type, bool Required);

internal class HeaderParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        var headers = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<HeaderMetadata>();

        foreach (var h in headers)
            AddHeaderParameter(operation, h);
    }

    private static void AddHeaderParameter(OpenApiOperation operation, HeaderMetadata header)
    {
        if (operation.Parameters.Any(p => p.In == ParameterLocation.Header && p.Name == header.HeaderName))
            return;

        operation.Parameters.Add(new()
        {
            Name = header.HeaderName,
            In = ParameterLocation.Header,
            Required = header.Required,
            Schema = new OpenApiSchema
            {
                Type = header.Type
            }
        });
    }
}