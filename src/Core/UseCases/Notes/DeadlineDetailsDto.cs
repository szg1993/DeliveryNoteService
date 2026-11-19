using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record DeadlineDetailsDto
{
    public int Id { get; init; }

    public DateOnly TakeoverDate { get; init; }

    public DateOnly ShipDate { get; init; }

    public DateOnly EstimatedArrivalDate { get; init; }

    public static DeadlineDetailsDto FromModel(NoteDeadline model)
    {
        return new DeadlineDetailsDto
        {
            Id = model.Id,
            TakeoverDate = model.TakeoverDate,
            ShipDate = model.ShipDate,
            EstimatedArrivalDate = model.EstimatedArrivalDate,
        };
    }
}