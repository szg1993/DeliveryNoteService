using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record CreatePackageDetailDto
{
    public decimal? TareWgt { get; init; }

    public string TareWgtUm { get; init; } = null!;

    public decimal? PkgQty { get; init; }

    public string PkgScale { get; init; } = null!;

    public decimal? PkgSizeX { get; init; }

    public decimal? PkgSizeY { get; init; }

    public decimal? PkgSizeZ { get; init; }

    public string PkgSizeUm { get; init; } = null!;

    public void Validate()
    {
        Guard.ThrowIfMissing(TareWgt);
        Guard.ThrowIfMissing(PkgQty);
        Guard.ThrowIfMissing(PkgSizeX);
        Guard.ThrowIfMissing(PkgSizeY);
        Guard.ThrowIfMissing(PkgSizeZ);
        if (PkgQty <= 0 || TareWgt < 0 || PkgSizeX < 0 || PkgSizeY < 0 || PkgSizeZ < 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid package quantity, weight or dimensions.");
        Guard.ThrowIfMissing(TareWgtUm);
        Guard.ThrowIfTooLong(TareWgtUm, NoteLimits.TareWgtUm);
        Guard.ThrowIfMissing(PkgScale);
        Guard.ThrowIfTooLong(PkgScale, NoteLimits.PkgScale);
        Guard.ThrowIfMissing(PkgSizeUm);
        Guard.ThrowIfTooLong(PkgSizeUm, NoteLimits.PkgSizeUm);
    }

    public NotePackageDetail ToModel()
        => MapTo(new NotePackageDetail());

    public NotePackageDetail MapTo(NotePackageDetail model)
    {
        model.TareWgt = TareWgt!.Value;
        model.TareWgtUm = TareWgtUm;
        model.PkgQty = PkgQty!.Value;
        model.PkgScale = PkgScale;
        model.PkgSizeX = PkgSizeX!.Value;
        model.PkgSizeY = PkgSizeY!.Value;
        model.PkgSizeZ = PkgSizeZ!.Value;
        model.PkgSizeUm = PkgSizeUm;

        return model;
    }
}