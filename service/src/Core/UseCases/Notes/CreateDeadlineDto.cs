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
        NoteInput.Require(TakeoverDate.HasValue, nameof(TakeoverDate));
        NoteInput.Require(ShipDate.HasValue, nameof(ShipDate));
        NoteInput.Require(EstimatedArrivalDate.HasValue, nameof(EstimatedArrivalDate));
        NoteInput.Require(TakeoverDate <= ShipDate && ShipDate <= EstimatedArrivalDate, "DeadlineOrder");
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