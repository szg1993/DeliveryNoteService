using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record CreateDeadlineDto
{
    public DateOnly? TakeoverDate { get; init; }

    public DateOnly? ShipDate { get; init; }

    public DateOnly? EstimatedArrivalDate { get; init; }

    public void Validate()
    {
        Guard.ThrowIfMissing(TakeoverDate);
        Guard.ThrowIfMissing(ShipDate);
        Guard.ThrowIfMissing(EstimatedArrivalDate);

        if (TakeoverDate > ShipDate || ShipDate > EstimatedArrivalDate)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid deadline order.");
    }

    public NoteDeadline ToModel()
        => MapTo(new NoteDeadline());

    public NoteDeadline MapTo(NoteDeadline model)
    {
        model.TakeoverDate = TakeoverDate!.Value;
        model.ShipDate = ShipDate!.Value;
        model.EstimatedArrivalDate = EstimatedArrivalDate!.Value;

        return model;
    }
}