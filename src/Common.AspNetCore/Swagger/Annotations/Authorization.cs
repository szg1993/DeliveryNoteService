using Common.AspNetCore.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.AspNetCore.Swagger.Annotations;

internal sealed class AuthorizationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var hasAuthorize = metadata.OfType<AuthorizeAttribute>().Any();

        if (!hasAuthorize)
            return;

        var schema = context.SchemaGenerator.GenerateSchema(
            typeof(MedusaErrorResponse),
            context.SchemaRepository);
        operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse
        {
            Description = "Unauthorized",
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        });
    }
}