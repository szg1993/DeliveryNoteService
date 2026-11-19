using Common.Core.Model;

namespace DeliveryNoteService.Core.Models;

public class NoteDeadline : IModel
{
    public int Id { get; set; }
    public int NoteId { get; set; }

    public DateOnly TakeoverDate { get; set; }

    public DateOnly ShipDate { get; set; }

    public DateOnly EstimatedArrivalDate { get; set; }

}