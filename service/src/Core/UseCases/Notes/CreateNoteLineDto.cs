using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
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
        Guard.ThrowIfMissing(PartCode?.Trim(), nameof(PartCode));
        Guard.ThrowIfTooLong(PartCode?.Trim(), NoteLimits.PartCode, nameof(PartCode));
        Guard.ThrowIfMissing(PartDesc?.Trim(), nameof(PartDesc));
        Guard.ThrowIfTooLong(PartDesc?.Trim(), NoteLimits.PartDesc, nameof(PartDesc));
        Guard.ThrowIfTooLong(PartCmt?.Trim(), NoteLimits.PartCmt, nameof(PartCmt));
        Guard.ThrowIfMissing(PartQty);
        Guard.ThrowIfMissing(PartQtyUm?.Trim(), nameof(PartQtyUm));
        Guard.ThrowIfTooLong(PartQtyUm?.Trim(), NoteLimits.PartQtyUm, nameof(PartQtyUm));
        Guard.ThrowIfMissing(PartWgt);
        if (PartQty <= 0 || PartWgt < 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid line quantity or weight.");
        Guard.ThrowIfMissing(PartWgtUm?.Trim(), nameof(PartWgtUm));
        Guard.ThrowIfTooLong(PartWgtUm?.Trim(), NoteLimits.PartWgtUm, nameof(PartWgtUm));
    }

    public NoteLine ToModel() => MapTo(new NoteLine());

    public NoteLine MapTo(NoteLine model)
    {
        model.PartCode = PartCode.Trim();
        model.PartDesc = PartDesc.Trim();
        model.PartCmt = string.IsNullOrWhiteSpace(PartCmt) ? null : PartCmt.Trim();
        model.PartQty = PartQty!.Value;
        model.PartQtyUm = PartQtyUm.Trim();
        model.PartWgt = PartWgt!.Value;
        model.PartWgtUm = PartWgtUm.Trim();

        return model;
    }
}