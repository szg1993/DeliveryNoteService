using Common.AspNetCore.ErrorHandling;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Recipients;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DeliveryNoteService.WebApp.RestApi;

public static class RecipientEndpoints
{
    public static IEndpointRouteBuilder MapRecipientRoutes(this IEndpointRouteBuilder app)
    {
        app
            .MapGet(
                "/api/recipients",
                async (
                    [AsParameters] PageRequestQueryParam pageRequest,
                    [AsParameters] Sorting<RecipientsSortBy> sorting,
                    [FromServices] GetRecipients getRecipients,
                    CancellationToken ct
                ) =>
                {
                    var pagination = pageRequest.ToUseCaseObject();
                    return await getRecipients.RunAsync(pagination, sorting, ct);
                }
            )
            .WithTags("Recipients")
            .WithName("GetRecipients")
            .WithSummary("List recipients")
            .WithDescription("Lists recipient details, with normalized code and country, immediately after creation. Defaults to page 1, limit 10, and ID ascending. SortBy accepts Id or Name; SortDir accepts Asc or Desc. Equal names are ordered by ID ascending in either direction. Limit 0 returns all recipients with page 1 metadata. Page must be positive and limit cannot be negative, including empty and unpaged requests. Invalid paging or sorting returns 400 in the standard error format. Authentication is not required.")
            .AllowAnonymous()
            .Produces<PagedEnvelope<RecipientDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest);

        app
            .MapGet(
                "/api/recipients/{id}",
                (
                    [FromRoute] int id,
                    [FromServices] GetRecipient getRecipient,
                    CancellationToken ct
                ) => getRecipient.RunAsync(id, ct)
            )
            .WithTags("Recipients")
            .WithName("GetRecipient")
            .WithSummary("Retrieve a recipient")
            .WithDescription("Retrieves recipient details, with normalized code and country, including the server-generated ID, UTC creation time, and derived foreign status. ID must be an integer. An ID with no matching recipient, including zero or negative values, returns 404. Malformed or out-of-range integer input returns 400. Authentication is not required.")
            .AllowAnonymous()
            .Produces<Envelope<RecipientDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound);

        app
            .MapPost(
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
            .WithDescription("Creates a delivery destination from six required fields. Code and country are trimmed and uppercased; name, ZIP, city, and address are stored exactly as supplied, and length limits apply to the supplied values. Duplicate names are allowed; a duplicate normalized code returns 400 with error code Recipient.AlreadyExists. Invalid input returns 400. The 201 response contains the created recipient and a Location header pointing to its detail endpoint. Authentication is not required.")
            .AllowAnonymous()
            .Accepts<CreateRecipientDto>("application/json")
            .Produces<Envelope<RecipientDetailsDto>>(StatusCodes.Status201Created)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Responses["201"].Headers["Location"] = new OpenApiHeader
                {
                    Description = "Relative URI of the created recipient's detail endpoint.",
                    Schema = new OpenApiSchema { Type = "string", Format = "uri-reference" }
                };
                return operation;
            });

        app
            .MapPut(
                "/api/recipients/{id}",
                async (
                    [FromRoute] int id,
                    [FromBody] UpdateRecipientDto dto,
                    [FromServices] UpdateRecipient updateRecipient,
                    CancellationToken ct
                ) => await updateRecipient.RunAsync(id, dto, ct)
            )
            .WithTags("Recipients")
            .WithName("UpdateRecipient")
            .WithSummary("Update a recipient")
            .WithDescription("Replaces a recipient's name, country, ZIP, city, and address; all five fields are required. Country is trimmed and uppercased, foreign status is re-derived from it, and the other text is stored exactly as supplied with the same limits as creation. The code, ID, and creation time never change; a supplied code or other unknown property is ignored. Concurrent updates are not checked: the last update wins. Invalid input or a malformed ID returns 400; an ID with no matching recipient, including zero or negative values, returns 404. The 200 response contains the updated recipient. Authentication is not required.")
            .AllowAnonymous()
            .Accepts<UpdateRecipientDto>("application/json")
            .Produces<Envelope<RecipientDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}