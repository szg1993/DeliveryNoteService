using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreateNoteLineDto
{
    public string PartCode { get; init; } = null!;

    public string PartDesc { get; init; } = null!;

    public string? PartCmt { get; init; }

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PartQty { get; init; }

    public string PartQtyUm { get; init; } = null!;

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PartWgt { get; init; }

    public string PartWgtUm { get; init; } = null!;

    public void Validate()
    {
        NoteInput.Text(PartCode, NoteLimits.PartCode, nameof(PartCode), false);
        NoteInput.Text(PartDesc, NoteLimits.PartDesc, nameof(PartDesc), false);
        NoteInput.Text(PartCmt, NoteLimits.PartCmt, nameof(PartCmt), true);
        NoteInput.Number(PartQty, nameof(PartQty), true);
        NoteInput.Text(PartQtyUm, NoteLimits.PartQtyUm, nameof(PartQtyUm), false);
        NoteInput.Number(PartWgt, nameof(PartWgt), false);
        NoteInput.Text(PartWgtUm, NoteLimits.PartWgtUm, nameof(PartWgtUm), false);
    }

    public NoteLine ToModel() => MapTo(new NoteLine());

    public NoteLine MapTo(NoteLine model)
    {
        model.PartCode = PartCode.Trim();
        model.PartDesc = PartDesc.Trim();
        model.PartCmt = NoteInput.Optional(PartCmt);
        model.PartQty = PartQty!.Value;
        model.PartQtyUm = PartQtyUm.Trim();
        model.PartWgt = PartWgt!.Value;
        model.PartWgtUm = PartWgtUm.Trim();

        return model;
    }
}