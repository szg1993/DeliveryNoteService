using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Medusa.Enap.Common.AspNetCore.Swagger.Annotations;

public static class OperationIdUtil
{
    public static RouteHandlerBuilder WithOperationId(this RouteHandlerBuilder builder, string operationId)
        => builder.WithMetadata(new OperationIdMetadata(operationId));
}

internal record OperationIdMetadata(string OperationId);

internal sealed class OperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        foreach (var m in endpointMetadata.OfType<OperationIdMetadata>())
            operation.OperationId = m.OperationId;
    }
}
