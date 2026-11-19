using Common.Core.Model;

namespace DeliveryNoteService.Core.Models;

public class NoteLine : IModel
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public int Position { get; set; }

    public string PartCode { get; set; } = null!;

    public string PartDesc { get; set; } = null!;

    public string? PartCmt { get; set; }

    public decimal PartQty { get; set; }

    public string PartQtyUm { get; set; } = null!;

    public decimal PartWgt { get; set; }

    public string PartWgtUm { get; set; } = null!;

}