using Medusa.Enap.Common.AspNetCore.ErrorHandling;
using Medusa.Enap.Common.AspNetCore.Swagger.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

namespace Medusa.Enap.Common.AspNetCore.Swagger.Annotations;

public static class ResponseUtils
{
    public static RouteHandlerBuilder ProducesOk<TResponse>(this RouteHandlerBuilder builder, string? description = null)
        => builder.Produces(StatusCodes.Status200OK, typeof(TResponse), description);

    public static RouteHandlerBuilder ProducesCreated<TResponse>(this RouteHandlerBuilder builder, string? description = null)
        => builder.Produces(StatusCodes.Status201Created, typeof(TResponse), description);

    public static RouteHandlerBuilder ProducesNoContent(this RouteHandlerBuilder builder, string? description = null)
        => builder.Produces(StatusCodes.Status204NoContent, responseType: null, description: description);

    public static RouteHandlerBuilder ProducesBadRequest(this RouteHandlerBuilder builder)
        => builder.Produces(StatusCodes.Status400BadRequest, typeof(MedusaErrorResponse), "Invalid request input data.");

    public static RouteHandlerBuilder ProducesUnauthorized(this RouteHandlerBuilder builder, string description = "Authentication required")
        => builder.Produces(StatusCodes.Status401Unauthorized, typeof(MedusaErrorResponse), description);

    public static RouteHandlerBuilder ProducesForbidden(this RouteHandlerBuilder builder)
        => builder.Produces(StatusCodes.Status403Forbidden, typeof(MedusaErrorResponse), "No permission.");

    public static RouteHandlerBuilder ProducesNotFound(this RouteHandlerBuilder builder, string description = "Resource not found")
        => builder.Produces(StatusCodes.Status404NotFound, typeof(MedusaErrorResponse), description);

    public static RouteHandlerBuilder ProducesUnprocessableEntity<TError>(this RouteHandlerBuilder builder, string? description = null)
        => builder.Produces(StatusCodes.Status422UnprocessableEntity, typeof(TError), description);

    public static RouteHandlerBuilder Produces(this RouteHandlerBuilder builder, int statusCode, Type? responseType, string? description = null)
    {
        // "contentType" is named argument to avoid ambiguity and recursion to this very same method
        builder.Produces(statusCode, responseType, contentType: responseType is null ? null : MediaTypeNames.Application.Json);
        builder.WithResponseDescription(statusCode, description);
        return builder;
    }
}