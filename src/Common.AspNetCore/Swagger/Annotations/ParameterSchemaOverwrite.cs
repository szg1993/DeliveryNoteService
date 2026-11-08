using Medusa.Enap.Common.Core.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Medusa.Enap.Common.AspNetCore.Swagger.Annotations;

public static class ParameterSchemaOverwriteUtil
{
    public static RouteHandlerBuilder WithParameterSchemaOverwrite(this RouteHandlerBuilder builder, string parameterName, string schemaType)
        => builder.WithMetadata(new ParameterSchemaOverwriteMetadata(parameterName, schemaType));
}

public record ParameterSchemaOverwriteMetadata(string Name, string Schema);

public sealed class ParameterSchemaOverwriteFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var schemaOverwrites = context.ApiDescription.ActionDescriptor
            .EndpointMetadata
            .OfType<ParameterSchemaOverwriteMetadata>()
            .ToDictionary(m => m.Name, m => m.Schema);

        if (schemaOverwrites.None())
            return;
        if (operation.Parameters is null)
            return;

        foreach (var p in operation.Parameters)
        {
            if (schemaOverwrites.TryGetValue(p.Name, out var schema))
                p.Schema = new OpenApiSchema { Type = schema };
        }
    }
}