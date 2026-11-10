using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record NoteLineDetailsDto
{
    public int Id { get; init; }

    public string PartCode { get; init; } = null!;

    public string PartDesc { get; init; } = null!;

    public string? PartCmt { get; init; }

    public decimal PartQty { get; init; }

    public string PartQtyUm { get; init; } = null!;

    public decimal PartWgt { get; init; }

    public string PartWgtUm { get; init; } = null!;

    public static NoteLineDetailsDto FromModel(NoteLine model)
    {
        return new NoteLineDetailsDto
        {
            Id = model.Id,
            PartCode = model.PartCode,
            PartDesc = model.PartDesc,
            PartCmt = model.PartCmt,
            PartQty = model.PartQty,
            PartQtyUm = model.PartQtyUm,
            PartWgt = model.PartWgt,
            PartWgtUm = model.PartWgtUm,
        };
    }
}