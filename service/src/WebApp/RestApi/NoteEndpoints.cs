using Common.AspNetCore.ErrorHandling;
using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryNoteService.WebApp.RestApi;

public static class NoteEndpoints
{
    public static IEndpointRouteBuilder MapNoteRoutes(this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/delivery-notes",
            async (
                [FromBody] CreateNoteDto dto,
                [FromServices] CreateNote createNote,
                CancellationToken ct
            ) => Results.Json(await createNote.RunAsync(dto, ct), statusCode: StatusCodes.Status201Created)
        )
            .WithTags("Delivery notes")
            .WithName("CreateNote")
            .WithSummary("Create a complete delivery note")
            .WithDescription("Creates a complete note for an existing recipient. Authentication is not required. Each POST creates separately, including retries. Text is trimmed before validation and storage; blank optional EKAER/comments become null. Unit labels are free text. All quantities must be positive; all weights and dimensions must be explicitly supplied and nonnegative, without rounding. All deadlines require yyyy-MM-dd calendar dates in takeover, shipment, arrival order; equal and past dates are accepted. Unknown properties and caller-assigned IDs, UserId, number, status and creation time are rejected at every request level; known names match case-insensitively. Server assigns IDs, UTC creation time and Pending status. Permanent numbers N00001..N99999 are unique; gaps are allowed, no reset or reuse. Exhaustion and invalid input return 400; an unknown positive recipient returns 404. Full current recipient details are returned.")
            .AllowAnonymous()
            .Accepts<CreateNoteDto>("application/json")
            .Produces<Envelope<NoteDetailsDto>>(StatusCodes.Status201Created)
            .Produces<MedusaErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<MedusaErrorResponse>(StatusCodes.Status404NotFound);
        return app;
    }
}