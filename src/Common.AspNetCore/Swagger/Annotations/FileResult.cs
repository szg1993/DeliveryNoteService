using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Net.Mime;

namespace Common.AspNetCore.Swagger.Annotations;

public static class FileResultUtil
{
    public static RouteHandlerBuilder ProducesFileCsv(this RouteHandlerBuilder builder, string? description = null)
        => builder.ProducesFile(MediaTypeNames.Text.Csv, description);

    public static RouteHandlerBuilder ProducesFileXml(this RouteHandlerBuilder builder, string? description = null)
        => builder.ProducesFile(MediaTypeNames.Application.Xml, description);

    public static RouteHandlerBuilder ProducesFileOctet(this RouteHandlerBuilder builder, string? description = null)
        => builder.ProducesFile(MediaTypeNames.Application.Octet, description);

    public static RouteHandlerBuilder ProducesFile(this RouteHandlerBuilder builder, string contentType, string? description = null)
        => builder.WithMetadata(new FileResultMetadata(contentType, description));
}

internal record FileResultMetadata(string ContentType, string? Description);

internal sealed class FileResultFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {

        var metaData = context.ApiDescription.ActionDescriptor
            .EndpointMetadata.OfType<FileResultMetadata>()
            .SingleOrDefault();

        if (metaData is null)
            return;

        operation.Responses["200"] = new OpenApiResponse
        {
            Headers = new Dictionary<string, OpenApiHeader>
            {
                {
                    "Content-Disposition",
                    new()
                    {
                        Description = "Attachment filename",
                        Schema = new OpenApiSchema { Type = "string" }
                    }
                },
                {
                    "Content-Length",
                    new() { Schema = new OpenApiSchema { Type = "integer", Format = "int64" } }
                }
            },
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    metaData.ContentType,
                    new() { Schema = new OpenApiSchema { Type = "string", Format = "binary" }}
                }
            },
            Description = metaData.Description ?? HttpStatusCode.OK.ToString()
        };

        SortResponses(operation);
    }

    private static void SortResponses(OpenApiOperation operation)
    {
        var responses = operation.Responses;
        operation.Responses = [];

        foreach (var item in responses.OrderBy(l => l.Key))
            operation.Responses[item.Key] = item.Value;
    }
}