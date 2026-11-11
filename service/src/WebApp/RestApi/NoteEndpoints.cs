using Common.AspNetCore.ErrorHandling;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace DeliveryNoteService.WebApp.RestApi;

public static class NoteEndpoints
{
    public static IEndpointRouteBuilder MapNoteRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/delivery-notes/{id}",
            async (
                [FromRoute] int id,
                [FromServices] GetDeliveryNote getDeliveryNote,
                CancellationToken ct
            ) => await getDeliveryNote.RunAsync(id, ct)
        )
            .WithTags("Delivery notes")
            .WithName("GetNote")
            .WithSummary("Retrieve a delivery note")
            .WithDescription("Retrieves the complete saved note, including server-assigned IDs, number, Pending status, UTC creation time, contact, package, calendar deadlines and original line order. Recipient details reflect the current linked recipient. Calendar dates are returned exactly as yyyy-MM-dd without timezone conversion. ID must be an integer. An absent note, including zero or negative IDs, returns 404; malformed or out-of-range integer input returns 400. Authentication is not required; full contact and recipient details are returned. Reading does not change the note.")
            .AllowAnonymous()
            .Produces<Envelope<NoteDetailsDto>>(StatusCodes.Status200OK)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound);

        app.MapPost(
            "/api/delivery-notes",
            async (
                [FromBody] CreateDeliveryNoteDto dto,
                [FromServices] CreateDeliveryNote createDeliveryNote,
                CancellationToken ct
            ) =>
            {
                var result = await createDeliveryNote.RunAsync(dto, ct);
                return Results.Created($"/api/delivery-notes/{result.Content.Id}", result);
            }
        )
            .WithTags("Delivery notes")
            .WithName("CreateNote")
            .WithSummary("Create a complete delivery note")
            .WithDescription("Creates a complete note for an existing recipient. Authentication is not required. Each POST creates separately, including retries. Text is trimmed before validation and storage; blank optional EKAER/comments become null. Unit labels are free text. All quantities must be positive; all weights and dimensions must be explicitly supplied and nonnegative, without rounding. All deadlines require yyyy-MM-dd calendar dates in takeover, shipment, arrival order; equal and past dates are accepted. Unknown properties and caller-assigned IDs, UserId, number, status and creation time are rejected at every request level; known names match case-insensitively. Server assigns IDs, UTC creation time and Pending status. Permanent numbers N00001..N99999 are unique; gaps are allowed, no reset or reuse. Exhaustion and invalid input return 400; an unknown positive recipient returns 404. Full current recipient details are returned. The 201 Location header points to the saved note detail endpoint.")
            .AllowAnonymous()
            .Accepts<CreateDeliveryNoteDto>("application/json")
            .Produces<Envelope<NoteDetailsDto>>(StatusCodes.Status201Created)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["201"].Headers["Location"] = new OpenApiHeader
                {
                    Description = "Relative URI of the created delivery note's detail endpoint.",
                    Schema = new OpenApiSchema { Type = "string", Format = "uri-reference" }
                };
                return operation;
            });
        return app;
    }
}