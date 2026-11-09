using Common.AspNetCore.ErrorHandling;
using Common.Core.Messaging;
using DeliveryNoteService.Application.UseCases.Recipients;
using DeliveryNoteService.Core.UseCases.Recipients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DeliveryNoteService.WebApp.RestApi;

public static class RecipientEndpoints
{
    public static IEndpointRouteBuilder MapRecipientRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/recipients",
            async (
                [AsParameters] PageRequestQueryParam pageRequest,
                [AsParameters] Sorting<RecipientsSortBy> sorting,
                [FromServices] GetRecipients getRecipients,
                CancellationToken ct
            ) => await getRecipients.RunAsync(pageRequest.ToUseCaseObject(), sorting, ct)
        )
            .WithTags("Recipients")
            .WithName("GetRecipients")
            .WithSummary("List recipients")
            .WithDescription("Lists normalized recipient details immediately after creation. Defaults to page 1, limit 10, and ID ascending. SortBy accepts Id or Name; SortDir accepts Asc or Desc. Equal names are ordered by ID ascending in either direction. Limit 0 returns all recipients with page 1 metadata. Page must be positive and limit cannot be negative, including empty and unpaged requests. Invalid paging or sorting returns 400 in the standard error format. Authentication is not required.")
            .AllowAnonymous()
            .Produces<PagedEnvelope<RecipientDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest);

        app.MapGet(
            "/api/recipients/{id}",
            async (
                [FromRoute] int id,
                [FromServices] GetRecipient getRecipient,
                CancellationToken ct
            ) => await getRecipient.RunAsync(id, ct)
        )
            .WithTags("Recipients")
            .WithName("GetRecipient")
            .WithSummary("Retrieve a recipient")
            .WithDescription("Retrieves normalized recipient details, including the server-generated ID, UTC creation time, and derived foreign status. ID must be a positive integer. Invalid input returns 400; an unknown positive ID returns 404. Authentication is not required.")
            .AllowAnonymous()
            .Produces<Envelope<RecipientDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound);

        app.MapPost(
            "/api/recipients",
            async (
                [FromBody] CreateRecipientDto dto,
                [FromServices] CreateRecipient createRecipient,
                CancellationToken ct
            ) =>
            {
                var result = await createRecipient.RunAsync(dto, ct);
                return Results.Created($"/api/recipients/{result.Content.Id}", result);
            }
        )
            .WithTags("Recipients")
            .WithName("CreateRecipient")
            .WithSummary("Create a recipient")
            .WithDescription("Creates a delivery destination from six required fields. Surrounding whitespace is removed; code and country are uppercased. Duplicate names are allowed; duplicate normalized codes return 409. Invalid input returns 400. The 201 response contains the created recipient and a Location header pointing to its detail endpoint. Authentication is not required.")
            .AllowAnonymous()
            .Accepts<CreateRecipientDto>("application/json")
            .Produces<Envelope<RecipientDetailsDto>>(StatusCodes.Status201Created)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status409Conflict)
            .WithOpenApi(operation =>
            {
                operation.Responses["201"].Headers["Location"] = new OpenApiHeader
                {
                    Description = "Relative URI of the created recipient's detail endpoint.",
                    Schema = new OpenApiSchema { Type = "string", Format = "uri-reference" }
                };
                return operation;
            });

        return app;
    }
}