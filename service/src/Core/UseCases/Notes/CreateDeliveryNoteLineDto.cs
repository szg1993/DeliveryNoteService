using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record CreateDeliveryNoteLineDto
{
    public string PartCode { get; init; } = null!;

    public string PartDesc { get; init; } = null!;

    public string? PartCmt { get; init; }

    public decimal? PartQty { get; init; }

    public string PartQtyUm { get; init; } = null!;

    public decimal? PartWgt { get; init; }

    public string PartWgtUm { get; init; } = null!;

    public void Validate()
    {
        Guard.ThrowIfMissing(PartCode);
        Guard.ThrowIfTooLong(PartCode, NoteLimits.PartCode);
        Guard.ThrowIfMissing(PartDesc);
        Guard.ThrowIfTooLong(PartDesc, NoteLimits.PartDesc);
        Guard.ThrowIfTooLong(PartCmt, NoteLimits.PartCmt);
        Guard.ThrowIfMissing(PartQty);
        Guard.ThrowIfMissing(PartQtyUm);
        Guard.ThrowIfTooLong(PartQtyUm, NoteLimits.PartQtyUm);
        Guard.ThrowIfMissing(PartWgt);
        if (PartQty <= 0 || PartWgt < 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid line quantity or weight.");
        Guard.ThrowIfMissing(PartWgtUm);
        Guard.ThrowIfTooLong(PartWgtUm, NoteLimits.PartWgtUm);
    }

    public NoteLine ToModel()
        => MapTo(new NoteLine());

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