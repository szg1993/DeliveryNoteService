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
        if (string.IsNullOrWhiteSpace(PartCode))
            throw new FieldMissingException(nameof(PartCode));
        Guard.ThrowIfTooLong(PartCode, NoteLimits.PartCode);
        if (string.IsNullOrWhiteSpace(PartDesc))
            throw new FieldMissingException(nameof(PartDesc));
        Guard.ThrowIfTooLong(PartDesc, NoteLimits.PartDesc);
        Guard.ThrowIfTooLong(PartCmt, NoteLimits.PartCmt);
        Guard.ThrowIfMissing(PartQty);
        if (string.IsNullOrWhiteSpace(PartQtyUm))
            throw new FieldMissingException(nameof(PartQtyUm));
        Guard.ThrowIfTooLong(PartQtyUm, NoteLimits.PartQtyUm);
        Guard.ThrowIfMissing(PartWgt);
        if (PartQty <= 0 || PartWgt < 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid line quantity or weight.");
        if (string.IsNullOrWhiteSpace(PartWgtUm))
            throw new FieldMissingException(nameof(PartWgtUm));
        Guard.ThrowIfTooLong(PartWgtUm, NoteLimits.PartWgtUm);
    }

    public NoteLine ToModel() => MapTo(new NoteLine());

    public NoteLine MapTo(NoteLine model)
    {
        model.PartCode = PartCode;
        model.PartDesc = PartDesc;
        model.PartCmt = PartCmt;
        model.PartQty = PartQty!.Value;
        model.PartQtyUm = PartQtyUm;
        model.PartWgt = PartWgt!.Value;
        model.PartWgtUm = PartWgtUm;

        return model;
    }
}