using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreateDeadlineDto
{
    [JsonConverter(typeof(StrictNoteDateConverter))]
    public DateOnly? TakeoverDate { get; init; }

    [JsonConverter(typeof(StrictNoteDateConverter))]
    public DateOnly? ShipDate { get; init; }

    [JsonConverter(typeof(StrictNoteDateConverter))]
    public DateOnly? EstimatedArrivalDate { get; init; }

    public void Validate()
    {
        Guard.ThrowIfMissing(TakeoverDate);
        Guard.ThrowIfMissing(ShipDate);
        Guard.ThrowIfMissing(EstimatedArrivalDate);
        if (TakeoverDate > ShipDate || ShipDate > EstimatedArrivalDate)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid deadline order.");
    }

    public NoteDeadline ToModel() => MapTo(new NoteDeadline());

    public NoteDeadline MapTo(NoteDeadline model)
    {
        model.TakeoverDate = TakeoverDate!.Value;
        model.ShipDate = ShipDate!.Value;
        model.EstimatedArrivalDate = EstimatedArrivalDate!.Value;

        return model;
    }
}